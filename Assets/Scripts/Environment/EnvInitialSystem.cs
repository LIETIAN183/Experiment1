using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
// using UnityEngine;

public class EnvInitialSystem : SystemBase
{
    public int index;
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        // 初始化数据，不能一开始系统就仿真，因为这样子可能还没开始物理计算，物体初始位置还在空中，没有落到实处，无法用于后续的重置
        Entities.WithAll<SubShakeData>().ForEach((ref SubShakeData subShakeData, in Rotation rotation, in Translation translation) =>
        {
            subShakeData.originLocalPosition = translation.Value;
        }).ScheduleParallel();

        // 用于分析数据，后续可以 hide，用于复位
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
}
