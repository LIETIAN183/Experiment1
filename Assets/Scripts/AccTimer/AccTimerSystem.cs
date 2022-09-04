using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using System.Threading.Tasks;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AccTimerSystem : SystemBase
{
    World simulation;

    protected override void OnCreate()
    {
        // 初始化时设置时间间隔0.04f，防止太卡
        simulation = World.DefaultGameObjectInjectionWorld;
        simulation.GetExistingSystem<FixedStepSimulationSystemGroup>().Timestep = 0.04f;
        this.Enabled = false;
    }

    protected override void OnStartRunning()
    {
        if (!simulation.GetExistingSystem<SetupBlobSystem>().dataReadSuccessed)
        {
            simulation.GetExistingSystem<UISystem>().DisplayNotificationForever("Data Read Error, Cant Exceed Simulation");
            this.Enabled = false;
            return;
        }
        // 设置仿真系统 Update 时间间隔
        // var fixedSimulationGroup = simulation?.GetExistingSystem<FixedStepSimulationSystemGroup>();
        // fixedSimulationGroup.Timestep = timeStep;
        var accTimer = GetSingleton<AccTimerData>();
        // 防止物理仿真 deltaTime 太小
        if (accTimer.simulationDeltaTime < 0.01f | accTimer.simulationDeltaTime > 0.06f) accTimer.simulationDeltaTime = 0.04f;
        simulation.GetExistingSystem<FixedStepSimulationSystemGroup>().Timestep = accTimer.simulationDeltaTime;

        // 初始化单例数据
        accTimer.seismicName = SetupBlobSystem.seismicBlobRefs[accTimer.seismicIndex].Value.seismicName.ToString();
        accTimer.dataDeltaTime = SetupBlobSystem.seismicBlobRefs[accTimer.seismicIndex].Value.dataDeltaTime;
        accTimer.accIndexInArray = 0;
        accTimer.increaseNumber = (int)(accTimer.simulationDeltaTime / accTimer.dataDeltaTime);
        accTimer.acc = float3.zero;
        accTimer.elapsedTime = 0;
        accTimer.seismicFinishTime = SetupBlobSystem.seismicBlobRefs[accTimer.seismicIndex].Value.seismicAccArray.Length * SetupBlobSystem.seismicBlobRefs[accTimer.seismicIndex].Value.dataDeltaTime;
        accTimer.curPGA = 0;
        accTimer.eventPGA = SetupBlobSystem.seismicBlobRefs[accTimer.seismicIndex].Value.seismicAccArray.ToArray().Max(a => math.length(a) / 9.8f);
        accTimer.magnitudeModification = math.select(1, accTimer.targetPGA / accTimer.eventPGA, accTimer.targetPGA != 0);
        if (accTimer.envEnhanceFactor.Equals(0)) accTimer.envEnhanceFactor = 1;
        SetSingleton(accTimer);

        SubSystemManager(true);
    }

    protected override void OnUpdate()
    {
        // 获得单例数据
        var accTimerData = GetSingleton<AccTimerData>();
        // 读取加速度序列
        ref BlobArray<float3> accArray = ref SetupBlobSystem.seismicBlobRefs[accTimerData.seismicIndex].Value.seismicAccArray;

        // 计算仿真开始时间
        accTimerData.elapsedTime = accTimerData.accIndexInArray * accTimerData.dataDeltaTime;
        if (accTimerData.elapsedTime < accTimerData.seismicFinishTime)
        {
            // 地震加速度生效
            accTimerData.acc = accArray[accTimerData.accIndexInArray] * accTimerData.magnitudeModification;
            accTimerData.curPGA = math.max(accTimerData.curPGA, math.length(accTimerData.acc) / 9.8f);
        }
        else
        {
            // 地震结束，仿真仍进行
            accTimerData.acc = float3.zero;
        }
        accTimerData.accIndexInArray += accTimerData.increaseNumber;
        // 更新单例数据
        SetSingleton(accTimerData);

        var simulationSetting = GetSingleton<SimulationLayerConfigurationData>();
        // 不进行统计时，设置仿真结束条件，延迟10s结束仿真
        if ((!simulationSetting.isPerformStatistics | !simulationSetting.isSimulateAgent) && accTimerData.elapsedTime >= accTimerData.seismicFinishTime + 2)
        {
            this.Enabled = false;
        }
    }

    public async void StartSingleSimulation(int index, float targetPGA)
    {
        // 确保开始仿真前已经生成智能体
        var simulationSetting = GetSingleton<SimulationLayerConfigurationData>();
        var spawnerData = GetSingleton<SpawnerData>();
        if (simulationSetting.isSimulateAgent && !spawnerData.canSpawn)
        {
            spawnerData.canSpawn = true;
            SetSingleton(spawnerData);
            simulation.GetExistingSystem<UISystem>().DisplayNotification2s("Spawning Agents");
            await Task.Delay(2000);
        }

        var accTimer = GetSingleton<AccTimerData>();
        accTimer.seismicIndex = index;
        accTimer.targetPGA = targetPGA;
        SetSingleton(accTimer);
        this.Enabled = true;
    }

    protected override void OnStopRunning()
    {
        SubSystemManager(false);
    }

    void SubSystemManager(bool state)
    {
        var setting = GetSingleton<SimulationLayerConfigurationData>();

        if (setting.isSimulateEnvironment)
        {
            // 环境系统
            simulation.GetExistingSystem<MCMotionSystem>().Enabled = state;
            simulation.GetExistingSystem<FCTopOscSystem>().Enabled = state;
        }

        if (setting.isSimulateFlowField)
        {
            // 路径算法
            simulation.GetExistingSystem<CalculateCostFieldSystem>().Enabled = state;
        }

        if (setting.isSimulateAgent)
        {
            // 行人状态切换
            simulation.GetExistingSystem<SeismicActiveSystem>().Enabled = state;
            simulation.GetExistingSystem<CheckReachedDestinationSystem>().Enabled = state;

            // 人群算法
            simulation.GetExistingSystem<AgentMovementSystem>().Enabled = state;
            // simulation.GetExistingSystem<SFMmovementSystem>().Enabled = state;
            // simulation.GetExistingSystem<SFMmovementSystem2>().Enabled = state;
            // simulation.GetExistingSystem<SFMmovementSystem3>().Enabled = state;

            if (setting.isDisplayTrajectories)
            {
                // 行人轨迹记录
                simulation.GetExistingSystem<TrajectoryRecordSystem>().Enabled = state;
            }
        }

        if (setting.isPerformStatistics)
        {
            // 统计系统
            simulation.GetExistingSystem<SingleStatisticSystem>().Enabled = state;
            simulation.GetExistingSystem<RecordSystem>().Enabled = state;
        }
    }
}