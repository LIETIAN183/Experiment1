using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
public class InitialSystem : SystemBase
{
    public int index;
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        // -------------------------------------------------- Environment ---------------------------------------------------------------------------
        // 初始化数据，不能一开始系统就仿真，因为这样子可能还没开始物理计算，物体初始位置还在空中，没有落到实处
        Random x = new Random();
        x.InitState();
        Entities.WithAll<ShakeData>().ForEach((ref ShakeData data) =>
        {
            data.k += x.NextFloat(-5, 5);
            data.c += x.NextFloat(-0.1f, 0.1f);
        }).ScheduleParallel();

        Entities.WithAll<SubShakeData>().ForEach((ref SubShakeData subShakeData, in Rotation rotation, in Translation translation) =>
        {
            subShakeData.originLocalPosition = translation.Value;
        }).ScheduleParallel();

        // 用于分析数据，后续可以 hide，用于复位
        Entities.WithAll<ComsData>().ForEach((ref ComsData data, in Translation translation, in Rotation rotation) =>
        {
            data.originPosition = translation.Value;
            data.originRotation = rotation.Value;
        }).ScheduleParallel();

        // -------------------------------------------------- Agent ---------------------------------------------------------------------------
        Entities.ForEach((ref AgentMovementData data) =>
        {
            data.state = AgentState.Delay;
            // TODO: 确定范围
            data.reactionTimeVariable = NormalDistribution.RandomGaussianInRange(0.7f, 1.3f);

        }).Run();

        // 开始仿真
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<AccTimerSystem>().Active(index);
        Enabled = false;
    }

    public void Active(int index)
    {
        this.Enabled = true;
        this.index = index;
    }
}
