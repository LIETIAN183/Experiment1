using System.Runtime.InteropServices;
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
    public static readonly float staticFriction = 0.3f;
    public static readonly float dynamicFriction = 0.5f;
    public static readonly float gravity = 9.81f;
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
        var horizontalAcc = math.length(acc.xz);
        float havokCoefficeitn = 1f;//0.05f;

        Entities
        .WithAll<ComsTag>()
        .WithName("ComsMove")
        .ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation, ref PhysicsMass physicsMass) =>
        {

            if (math.abs(physicsVelocity.Linear.y) < 0.001)
            {
                // 物体不离地时添加垂直惯性力
                physicsVelocity.ApplyLinearImpulse(physicsMass, -verticalAcc / physicsMass.InverseMass * time);
                // 添加水平惯性力
                if (translation.Value.y > 0.2f)
                {
                    physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, -acc / physicsMass.InverseMass * time, physicsMass.CenterOfMass);
                }
                else
                {
                    // 计算水平惯性力和摩擦力
                    if (math.length(physicsVelocity.Linear.xz) < 0.001)
                    {
                        var finalAcc = horizontalAcc - staticFriction * (gravity + verticalAcc.y);
                        if (finalAcc > 0)
                        {
                            physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, -math.normalize(acc) * finalAcc / physicsMass.InverseMass * time, physicsMass.CenterOfMass);
                        }
                    }
                    else
                    {
                        var finalAcc = horizontalAcc - dynamicFriction * (gravity + verticalAcc.y);
                        if (finalAcc > 0)
                        {
                            physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, -math.normalize(acc) * finalAcc / physicsMass.InverseMass * time, physicsMass.CenterOfMass);
                        }
                    }
                }
            }
        }).ScheduleParallel();
    }
}
