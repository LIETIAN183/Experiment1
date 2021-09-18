using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Physics;

public class EnvInitialSystem : SystemBase
{
    public int index;
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        // 初始化数据，不能一开始系统就仿真，因为这样子可能还没开始物理计算，物体初始位置还在空中，没有落到实处
        Entities.WithAll<SubShakeData>().ForEach((ref SubShakeData subBendData, in Rotation rotation, in Translation translation) =>
        {
            subBendData.originLocalPosition = translation.Value;
        }).ScheduleParallel();

        // 用于分析数据，后续可以 hide 
        Entities.WithAll<ComsTag>().ForEach((ref ComsTag data, in Translation translation) =>
        {
            data.originPosition = translation.Value;
        }).ScheduleParallel();

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
