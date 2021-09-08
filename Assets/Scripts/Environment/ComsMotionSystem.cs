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
        var time = Time.DeltaTime;
        float3 verticalAcc = new float3(0, acc.y, 0);
        acc.y = 0;
        float havokCoefficeitn = 1f;//0.05f;
        // PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        Entities
        .WithAll<ComsTag>()
        .WithName("ComsMove")
        .ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation, ref PhysicsMass physicsMass) =>
        {
            //判断是否在空中，垂直速度低于阈值表示物体既没有向上运动也没有下降运动，表示下面有物体支撑
            if (math.abs(physicsVelocity.Linear.y) <= 0.001)
            {
                // 可能施加力的方向是 Local 的导致无故弹跳
                physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, -acc / physicsMass.InverseMass * time, physicsMass.CenterOfMass);
                physicsVelocity.ApplyLinearImpulse(physicsMass, -verticalAcc / physicsMass.InverseMass * time);
            }
            else
            {
                //添加空气阻力
            }
        }).ScheduleParallel();
    }
}
