using Unity.Entities;
using Unity.Physics;

[AlwaysSynchronizeSystem]
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
    }
}
