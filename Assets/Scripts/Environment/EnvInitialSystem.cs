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

        // 货架由于需要子碰撞体运动，自身不能设置为刚体，所以需要跟随一个不可见的纯货架运动，一开始不设置的话，第一次的delta就会等于目标物的坐标，导致物体大量变化
        Entities.ForEach((ref AimTag aim, in Translation translation) =>
        {
            aim.lastPosition = translation.Value;
        }).Run();

        // 用于分析数据，后续可以 hide 
        Entities.WithAll<ComsTag>().ForEach((ref ComsTag data, in Translation translation, in Rotation rotation) =>
        {
            data.originPosition = translation.Value;
            data.originRotation = rotation.Value;
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

    // 分析
    public void Reload()
    {
        Entities.WithAll<ComsTag>().ForEach((ref Translation translation, ref Rotation rotation, in ComsTag data) =>
        {
            translation.Value = data.originPosition;
            rotation.Value = data.originRotation;
        }).ScheduleParallel();
    }
}
