using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Transforms;

// [AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class ComsMotionSystem : SystemBase
{
    public static readonly float staticFriction = 0.5f;
    public static readonly float gravity = -9.81f;
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var horiAcc = GetSingleton<AccTimerData>().acc;
        var vertiAcc = horiAcc.y;
        horiAcc.y = 0;
        var time = Time.DeltaTime;

        Entities
        .WithAll<ComsTag>()
        .WithName("ComsMove")
        .ForEach((ref PhysicsVelocity physicsVelocity, in Translation translation, in Rotation rotation, in PhysicsMass physicsMass) =>
        {
            // 垂直惯性力的添加通过修改全局
            // 垂直速度大于 0 时代表物体在空中运动
            if (math.abs(physicsVelocity.Linear.y) < 0.001)
            {
                // 当物体静止且水平惯性力小于最大静摩擦力时，直接不添加力，简化计算
                // 即当物体运动或者水平惯性力大于最大静摩擦力时，才添加水平惯性力到物体上
                // ma_{h}<μm(g-a_{v}) 表示水平惯性力小于最大静摩擦力

                if (math.length(physicsVelocity.Linear.xz) >= 0.001 || math.length(horiAcc) >= math.abs(staticFriction * (gravity - horiAcc.y)))
                {
                    physicsVelocity.ApplyLinearImpulse(physicsMass, -horiAcc / physicsMass.InverseMass * time);
                }
            }
            else
            {
                // 添加空气阻力
            }
        }).ScheduleParallel();
    }
}
