using Unity.Entities;
using Unity.Mathematics;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class AccTimerSystem : SystemBase
{
    World simulation;

    private float timeStep = 0.02f;

    public bool flag = true;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;

        // 设置读取加速度的功能类为单例模式，方便数据同步和读取
        RequireSingletonForUpdate<AccTimerData>();
        var entity = EntityManager.CreateEntity(typeof(AccTimerData));
        // 设置仿真系统 Update 时间间隔
        var fixedSimulationGroup = simulation?.GetExistingSystem<FixedStepSimulationSystemGroup>();
        fixedSimulationGroup.Timestep = timeStep;

        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        // 获得单例数据
        var accTimer = GetSingleton<AccTimerData>();
        var cofficient = GetSingleton<AnalysisTypeData>().cofficient / 100f;
        // 读取加速度序列
        ref BlobArray<GroundMotion> gmArray = ref SetupBlobSystem.gmBlobRefs[accTimer.gmIndex].Value.gmArray;

        // 超出时间范围后地震仿真结束
        // 放在函数末尾会导致时间还在最后一秒时由于 timeCount++ 直接超出时间范围直接结束仿真
        // 放在这里可以保留最后一秒的仿真数据
        if (accTimer.timeCount >= gmArray.Length)
        {
            accTimer.elapsedTime = accTimer.timeCount * accTimer.dataDeltaTime;
            ECSUIController.Instance.progress.currentTime = accTimer.elapsedTime;
            accTimer.acc = float3.zero;
            accTimer.timeCount += accTimer.increaseNumber;
        }
        else
        {
            accTimer.elapsedTime = accTimer.timeCount * accTimer.dataDeltaTime;
            // 更新时间进度条
            ECSUIController.Instance.progress.currentTime = accTimer.elapsedTime;
            accTimer.acc = gmArray[accTimer.timeCount].acceleration * cofficient;
            accTimer.timeCount += accTimer.increaseNumber;

            accTimer.pga = math.max(accTimer.pga, math.length(accTimer.acc));

        }
        // 更新单例数据
        SetSingleton(accTimer);
    }

    public void Active(int index)
    {
        // 初始化单例数据
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.gmIndex = index;
        accTimer.acc = 0;
        accTimer.timeCount = 0;
        accTimer.dataDeltaTime = SetupBlobSystem.gmBlobRefs[index].Value.deltaTime;
        accTimer.increaseNumber = (int)(timeStep / accTimer.dataDeltaTime);
        SetSingleton(accTimer);
        this.Enabled = true;

        simulation.GetExistingSystem<AnalysisSystem>().Enabled = true;
    }

    protected override void OnStopRunning()
    {
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.acc = 0;
        SetSingleton(accTimer);
    }
}
