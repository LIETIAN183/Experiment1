using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class ComsMotionSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var acc = GetSingleton<AccTimerData>().acc;
        float3 verticalAcc = new float3(0, acc.y, 0);
        acc.y = 0;
        float havokCoefficeitn = 0.05f;
        // PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        Entities
        .WithAll<ComsTag>()
        .WithName("ComsMove")
        .ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation, ref PhysicsMass physicsMass) =>
        {
            // 可能施加力的方向是 Local 的导致无故弹跳
            physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, acc / physicsMass.InverseMass * 0.01f * havokCoefficeitn, physicsMass.CenterOfMass);
            physicsVelocity.ApplyLinearImpulse(physicsMass, verticalAcc / physicsMass.InverseMass * 0.01f);
        }).ScheduleParallel();
    }
}
