using Unity.Entities;
// using Unity.Physics;
using Unity.Mathematics;
// using BansheeGz.BGDatabase;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;
// using UnityEngine;
// using UnityEngine;
// using Random = Unity.Mathematics.Random;
using System.Threading.Tasks;
using Unity.Burst;

// 程序一开始就运行该系统，放在 FixedStepSimulationSystemGroup 可能出现无法读到单例数据的错误
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [DisableAutoCreation]
public partial class MultiRoundStatisticsSystem : SystemBase
{
    World simulation;

    private EndSimulationEntityCommandBufferSystem m_EndSimECBSystem;

    // private Hash128 envGUID;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;
        m_EndSimECBSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
        this.Enabled = false;
    }

    protected override void OnStartRunning()
    {
        var analysisCircledata = GetSingleton<MultiRoundStatisticsData>();
        analysisCircledata.curStage = AnalysisStage.DataBackup;
        analysisCircledata.curSeismicIndex = 0;
        analysisCircledata.seismicEventsCount = SetupBlobSystem.seismicBlobRefs.Count;
        analysisCircledata.curSimulationTargetPGA = 0;
        SetSingleton<MultiRoundStatisticsData>(analysisCircledata);
    }

    // 内部不能使用异步的 aync 和 Task.Delay,因为该函数定时调用多次，会导致延时失效，反而相同函数执行多次，只适用于单次调用的函数
    protected override void OnUpdate()
    {
        // UnityEngine.Debug.Log(envGUID.ToString());
        var analysisCircledata = GetSingleton<MultiRoundStatisticsData>();

        switch (analysisCircledata.curStage)
        {
            case AnalysisStage.DataBackup:
                DataBuckup().Complete();
                analysisCircledata.curStage = AnalysisStage.Start;
                break;
            case AnalysisStage.Start:
                // 仿真下标超过地震事件数目，结束仿真
                if (analysisCircledata.curSeismicIndex >= analysisCircledata.seismicEventsCount)
                {
                    simulation.GetExistingSystemManaged<SingleStatisticSystem>().ExportData();
                    simulation.GetExistingSystemManaged<UISystem>().DisplayNotificationForever("Simulation Finished");
                    this.Enabled = false;
                    return;
                }
                else
                {
                    // 配置 仿真系统参数
                    // var accTimerData = GetSingleton<AccTimerData>();
                    if (analysisCircledata.pgaThreshold.Equals(0) | analysisCircledata.pgaStep.Equals(0))
                    {
                        // 设置当前仿真参数
                        simulation.GetExistingSystemManaged<AccTimerSystem>().StartSingleSimulation(analysisCircledata.curSeismicIndex++, analysisCircledata.pgaThreshold);
                    }
                    else
                    {
                        analysisCircledata.curSimulationTargetPGA += analysisCircledata.pgaStep;
                        // 单个地震事件结束，选择下一个地震事件
                        if (analysisCircledata.curSimulationTargetPGA > analysisCircledata.pgaThreshold)
                        {
                            // 当 pgaStep>pgaThreshold 时只会循环跳过，不进行仿真
                            analysisCircledata.curSeismicIndex++;
                            analysisCircledata.curSimulationTargetPGA = 0;
                            break;
                        }
                        // 设置当前仿真参数
                        simulation.GetExistingSystemManaged<AccTimerSystem>().StartSingleSimulation(analysisCircledata.curSeismicIndex, analysisCircledata.curSimulationTargetPGA);
                    }
                    // 启动
                    analysisCircledata.curStage = AnalysisStage.Simulation;
                }
                break;
            case AnalysisStage.Simulation:
                if (!simulation.GetExistingSystemManaged<AccTimerSystem>().Enabled)
                {
                    analysisCircledata.curStage = AnalysisStage.Recover;
                }
                break;
            case AnalysisStage.Recover:
                RecoverData().Complete();
                analysisCircledata.curStage = AnalysisStage.Start;
                break;
            default:
                break;
        }
        SetSingleton<MultiRoundStatisticsData>(analysisCircledata);
    }

    public async void StartMultiRoundStatistics(float pgaThreshold, float pgaStep)
    {
        // 确保开始仿真前已经生成智能体
        var simulationSetting = GetSingleton<SimulationLayerConfigurationData>();
        var spawnerData = GetSingleton<SpawnerData>();
        if (simulationSetting.isSimulateAgent && !spawnerData.canSpawn)
        {
            spawnerData.canSpawn = true;
            SetSingleton(spawnerData);
            simulation.GetExistingSystemManaged<UISystem>().DisplayNotification2s("Spawning Agents");
            await Task.Delay(2000);
        }
        // 开始仿真
        var analysisCircledata = GetSingleton<MultiRoundStatisticsData>();
        analysisCircledata.pgaThreshold = pgaThreshold;
        analysisCircledata.pgaStep = pgaStep;
        SetSingleton(analysisCircledata);
        this.Enabled = true;
    }

    [BurstCompile]
    public JobHandle DataBuckup()
    {
        // 最开始需要做的初始化
        simulation.GetExistingSystemManaged<SingleStatisticSystem>().ClearDataStorage();

        var ecb = m_EndSimECBSystem.CreateCommandBuffer().AsParallelWriter();

        Random x = new Random();
        x.InitState();
        var handle1 = Entities.WithAll<FCData>().ForEach((ref FCData data) =>
        {
            data.k += x.NextFloat(-5, 5);
            data.c += x.NextFloat(-0.2f, 0.2f);
        }).ScheduleParallel(Dependency);

        // 备份初始位置
        // var handle2 = Entities.WithAny<SubFCData, MCData, AgentMovementData>().ForEach((Entity e, int entityInQueryIndex, in Translation translation, in Rotation rotation) =>
        // {
        //     ecb.AddComponent<BackupData>(entityInQueryIndex, e, new BackupData { originPosition = translation.Value, originRotation = rotation.Value });
        // }).ScheduleParallel(Dependency);
        var handle2 = Entities.WithAny<AgentMovementData>().ForEach((Entity e, int entityInQueryIndex, in LocalTransform localTransform) =>
        {
            ecb.AddComponent<BackupData>(entityInQueryIndex, e, new BackupData { originPosition = localTransform.Position, originRotation = localTransform.Rotation });
        }).ScheduleParallel(Dependency);

        var sumHandle = JobHandle.CombineDependencies(handle1, handle2);

        m_EndSimECBSystem.AddJobHandleForProducer(sumHandle);

        Dependency = sumHandle;

        return sumHandle;
    }

    [BurstCompile]
    public JobHandle RecoverData()
    {
        // 重置环境
        simulation.GetExistingSystemManaged<InitialSystem>().ReloadSubScene();
        simulation.GetExistingSystemManaged<ReplaceSystem>().RemoveAllFluid();
        // 重置行人数据
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.targetPGA = 0;
        SetSingleton(accTimer);

        var ecb = m_EndSimECBSystem.CreateCommandBuffer().AsParallelWriter();

        var handle1 = Entities.WithAll<BackupData>().WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities).ForEach((ref LocalTransform localTransform, ref PhysicsVelocity velocity, in BackupData backup) =>
        {
            localTransform.Position = backup.originPosition;
            localTransform.Rotation = backup.originRotation;
            velocity.Linear = float3.zero;
            velocity.Angular = float3.zero;
        }).ScheduleParallel(Dependency);

        var handle2 = Entities.WithAll<Escaped>().WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities).ForEach((Entity e, int entityInQueryIndex) =>
        {
            ecb.AddComponent<Idle>(entityInQueryIndex, e);
            ecb.RemoveComponent<Escaped>(entityInQueryIndex, e);
            ecb.RemoveComponent<Disabled>(entityInQueryIndex, e);
        }).ScheduleParallel(Dependency);

        m_EndSimECBSystem.AddJobHandleForProducer(handle2);

        Dependency = handle2;

        return JobHandle.CombineDependencies(handle1, handle2);
    }
}
