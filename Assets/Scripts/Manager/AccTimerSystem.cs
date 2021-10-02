using Unity.Entities;
using Unity.Mathematics;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class AccTimerSystem : SystemBase
{
    World simulation;
    private int increase;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;

        // 设置读取加速度的功能类为单例模式，方便数据同步和读取
        RequireSingletonForUpdate<AccTimerData>();
        var entity = EntityManager.CreateEntity(typeof(AccTimerData));
        // EntityManager.SetName(entity, "AccTimer");
        // 设置仿真系统 Update 时间间隔
        var fixedSimulationGroup = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FixedStepSimulationSystemGroup>();
        fixedSimulationGroup.Timestep = 0.02f;
        increase = (int)(fixedSimulationGroup.Timestep / 0.01f);
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        // 获得单例数据
        var accTimer = GetSingleton<AccTimerData>();
        // 读取加速度序列
        ref BlobArray<GroundMotion> gmArray = ref SetupBlobSystem.gmBlobRefs[accTimer.gmIndex].Value.gmArray;

        // 超出时间范围后地震仿真结束
        // 放在函数末尾会导致时间还在最后一秒时由于 timeCount++ 直接超出时间范围直接结束仿真
        // 放在这里可以保留最后一秒的仿真数据
        if (accTimer.timeCount >= gmArray.Length)
        {
            // 关闭系统
            simulation.GetExistingSystem<ComsMotionSystem>().Enabled = false;
            this.Enabled = false;
        }
        else
        {
            // 更新加速度后，更新时间计量
            accTimer.acc = gmArray[accTimer.timeCount].acceleration;
            accTimer.timeCount += increase;
            // 更新单例数据
            SetSingleton(accTimer);
        }
    }

    public void Active(int index)
    {
        // 初始化单例数据
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.gmIndex = index;
        accTimer.acc = float3.zero;
        accTimer.timeCount = 0;
        SetSingleton(accTimer);
        this.Enabled = true;

        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = true;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = true;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = true;
    }

    protected override void OnStopRunning()
    {
        var accTimer = GetSingleton<AccTimerData>();
        accTimer.acc = float3.zero;
        SetSingleton(accTimer);
    }
}
