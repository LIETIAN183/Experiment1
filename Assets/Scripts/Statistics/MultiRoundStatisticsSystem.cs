using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;
using Unity.Burst;
using Unity.Collections;

// 程序一开始就运行该系统，放在 FixedStepSimulationSystemGroup 可能出现无法读到单例数据的错误
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [DisableAutoCreation]
public partial class MultiRoundStatisticsSystem : SystemBase
{
    private int counter;
    World managedWorld;
    WorldUnmanaged unmanagedWorld;
    private bool recoverSceneTag;

    protected override void OnCreate()
    {
        managedWorld = World.DefaultGameObjectInjectionWorld;
        unmanagedWorld = managedWorld.Unmanaged;
        recoverSceneTag = false;
        this.Enabled = false;
        counter = -1;
    }

    protected override void OnStartRunning()
    {
        var analysisCircledata = SystemAPI.GetSingleton<MultiRoundStatisticsData>();
        analysisCircledata.curStage = AnalysisStage.DataBackup;
        analysisCircledata.curSeismicIndex = 0;
        analysisCircledata.seismicEventsCount =
        SystemAPI.GetSingletonBuffer<BlobRefBuffer>().Length;
        analysisCircledata.curSimulationTargetPGA = 0;
        SystemAPI.SetSingleton<MultiRoundStatisticsData>(analysisCircledata);
    }

    // 内部不能使用异步的 aync 和 Task.Delay,因为该函数定时调用多次，会导致延时失效，反而相同函数执行多次，只适用于单次调用的函数
    protected override void OnUpdate()
    {
        var analysisCircledata = SystemAPI.GetSingleton<MultiRoundStatisticsData>();

        switch (analysisCircledata.curStage)
        {
            case AnalysisStage.DataBackup:
                DataBuckup();
                analysisCircledata.curStage = AnalysisStage.Start;
                break;
            case AnalysisStage.Start:

                // Normal
                // 仿真下标超过地震事件数目，结束仿真
                // if (analysisCircledata.curSeismicIndex >= analysisCircledata.seismicEventsCount)
                // {
                //     var handle = unmanagedWorld.GetExistingUnmanagedSystem<SingleStatisticSystem>();
                //     unmanagedWorld.GetUnsafeSystemRef<SingleStatisticSystem>(handle).ExportData();

                //     SystemAPI.SetSingleton(new MessageEvent
                //     {
                //         isActivate = true,
                //         message = "Simulation Finished",
                //         displayForever = true
                //     });
                //     this.Enabled = false;
                //     return;
                // }
                // else
                // {
                //     // 配置 仿真系统参数
                //     if (analysisCircledata.pgaThreshold.Equals(0) | analysisCircledata.pgaStep.Equals(0))
                //     {
                //         SystemAPI.SetSingleton(new StartSeismicEvent
                //         {
                //             isActivate = true,
                //             index = analysisCircledata.curSeismicIndex++,
                //             targetPGA = analysisCircledata.pgaThreshold
                //         });
                //     }
                //     else
                //     {
                //         analysisCircledata.curSimulationTargetPGA += analysisCircledata.pgaStep;
                //         // 单个地震事件结束，选择下一个地震事件
                //         if (analysisCircledata.curSimulationTargetPGA > analysisCircledata.pgaThreshold)
                //         {
                //             // 当 pgaStep>pgaThreshold 时只会循环跳过，不进行仿真
                //             analysisCircledata.curSeismicIndex++;
                //             analysisCircledata.curSimulationTargetPGA = 0;
                //             break;
                //         }
                //         // 设置当前仿真参数
                //         SystemAPI.SetSingleton(new StartSeismicEvent
                //         {
                //             isActivate = true,
                //             index = analysisCircledata.curSeismicIndex,
                //             targetPGA = analysisCircledata.curSimulationTargetPGA
                //         });
                //     }

                //     // 更新状态
                //     analysisCircledata.curStage = AnalysisStage.Simulation;
                // }




                if (counter >= 3)
                {
                    if (SystemAPI.GetSingleton<SimConfigData>().performStatistics)
                    {
                        var handle = unmanagedWorld.GetExistingUnmanagedSystem<SingleStatisticSystem>();
                        unmanagedWorld.GetUnsafeSystemRef<SingleStatisticSystem>(handle).ExportData();
                    }

                    SystemAPI.SetSingleton(new MessageEvent
                    {
                        isActivate = true,
                        message = "Simulation Finished",
                        displayForever = true
                    });
                    this.Enabled = false;
                    return;
                }
                else
                {
                    var set = SystemAPI.GetSingleton<FlowFieldSettingData>();
                    counter++;
                    switch (counter)
                    {
                        case 0:
                            set.index = 0;
                            break;
                        case 1:
                            set.index = 1;
                            break;
                        case 2:
                            set.index = 2;
                            break;
                        case 3:
                            set.index = 3;
                            break;
                        default:
                            break;
                    }
                    SystemAPI.SetSingleton(set);

                    // 配置 仿真系统参数

                    SystemAPI.SetSingleton(new StartSeismicEvent
                    {
                        isActivate = true,
                        index = 0,
                        targetPGA = 0
                    });

                    // 更新状态
                    analysisCircledata.curStage = AnalysisStage.Simulation;
                }
                break;
            case AnalysisStage.Simulation:
                if (!managedWorld.Unmanaged.GetExistingSystemState<TimerSystem>().Enabled)
                {
                    analysisCircledata.curStage = AnalysisStage.Recover;
                }
                break;
            case AnalysisStage.Recover:
                if (!recoverSceneTag)
                {
                    managedWorld.GetExistingSystemManaged<SimInitializeSystem>().ReloadSubScene();
                    recoverSceneTag = true;
                }
                else
                {
                    if (managedWorld.GetExistingSystemManaged<SimInitializeSystem>().SceneLoadState())
                    {
                        RecoverData();
                        recoverSceneTag = false;
                        analysisCircledata.curStage = AnalysisStage.Start;
                    }
                }
                break;
            default:
                break;
        }
        SystemAPI.SetSingleton(analysisCircledata);
    }

    public void StartMultiRoundStatistics(float pgaThreshold, float pgaStep)
    {
        var simulationSetting = SystemAPI.GetSingleton<SimConfigData>();
        var spawnerData = SystemAPI.GetSingleton<SpawnerData>();
        if (simulationSetting.simAgent && spawnerData.currentCount < spawnerData.desireCount)
        {
            // 这里需要 Allocator.Persistent，因为生成 Agent 需要花费较多时间， Allocator.TempJob 时间不够
            var ecb = new EntityCommandBuffer(Allocator.Persistent);
            this.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();

            new SpawnerAgentJob
            {
                ecb = ecb,
                physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                randomInitSeed = (uint)(SystemAPI.GetSingleton<RandomSeed>().seed + SystemAPI.Time.ElapsedTime.GetHashCode()),
            }.Schedule(Dependency).Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();

            var message = SystemAPI.GetSingleton<MessageEvent>();
            message.isActivate = true;
            message.message = "Spawning Agents";
            message.displayForever = false;
            SystemAPI.SetSingleton(message);
        }
        // 开始仿真
        var analysisCircledata = SystemAPI.GetSingleton<MultiRoundStatisticsData>();
        analysisCircledata.pgaThreshold = pgaThreshold;
        analysisCircledata.pgaStep = pgaStep;
        SystemAPI.SetSingleton(analysisCircledata);
        this.Enabled = true;
    }

    [BurstCompile]
    public void DataBuckup()
    {
        // 最开始需要做的初始化
        var handle = unmanagedWorld.GetExistingUnmanagedSystem<SingleStatisticSystem>();
        unmanagedWorld.GetUnsafeSystemRef<SingleStatisticSystem>(handle).ClearDataStorage();

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        new AgentBackupJob
        {
            parallelECB = ecb.AsParallelWriter()
        }.ScheduleParallel(Dependency).Complete();
        ecb.Playback(this.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void RecoverData()
    {
        // 重置环境
        SystemAPI.SetSingleton<ClearFluidEvent>(new ClearFluidEvent { isActivate = true });
        // 重置行人数据
        SystemAPI.SetSingleton(new TimerData());

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        if (SystemAPI.HasComponent<PhysicsVelocity>(SystemAPI.GetSingleton<SpawnerData>().prefab))
        {
            new AgentRecoverJob
            {
                idleList = SystemAPI.GetComponentLookup<Idle>(),
                escapedList = SystemAPI.GetComponentLookup<Escaped>(),
                parallelECB = ecb.AsParallelWriter()
            }.ScheduleParallel(Dependency).Complete();
        }
        else
        {
            new FlowFieldAgentRecoverJob
            {
                idleList = SystemAPI.GetComponentLookup<Idle>(),
                escapedList = SystemAPI.GetComponentLookup<Escaped>(),
                parallelECB = ecb.AsParallelWriter()
            }.ScheduleParallel(Dependency).Complete();
        }
        ecb.Playback(this.EntityManager);
        ecb.Dispose();
    }
}
