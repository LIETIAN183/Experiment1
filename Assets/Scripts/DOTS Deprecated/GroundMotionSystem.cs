using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;

// https://docs.unity3d.com/Packages/com.unity.entities@0.2/api/Unity.Entities.AlwaysSynchronizeSystemAttribute.html
// 就是依赖执行完后再执行自身，但这里是否需要这个还不清楚
[AlwaysSynchronizeSystem]
[DisableAutoCreation]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class GroundMotionSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var acc = GetSingleton<AccTimerData>().acc;

        Entities.WithAll<GroundTag>().WithName("GroundMotion").ForEach((ref PhysicsVelocity physicsVelocity) =>
        {
            // DeltaTime 为 0.01f ,因为已经设置了 DeltaTime 为固定值，那就不用每次再获取 DeltaTime了
            physicsVelocity.Linear += acc * 0.01f;
        }).ScheduleParallel();

        Entities.WithAll<GroundTag>().ForEach((ref Translation translation, ref GroundTag data) =>
        {
            // DeltaTime 为 0.01f ,因为已经设置了 DeltaTime 为固定值，那就不用每次再获取 DeltaTime了
            data.velocity += acc * 0.01f;
            translation.Value += data.velocity * 0.01f;
        }).ScheduleParallel();
    }
}
