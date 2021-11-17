using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Transforms;

// [AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(GlobalGravitySystem))]
public class ComsMotionSystem : SystemBase
{
    public static readonly float staticFriction = 0.5f;
    public static readonly float gravity = -9.81f;
    public static readonly float threshold = 0.0001f;
    protected override void OnCreate() => this.Enabled = false;

    protected override void OnUpdate()
    {
        var horiAcc = GetSingleton<AccTimerData>().acc;
        var vertiAcc = horiAcc.y;
        horiAcc.y = 0;
        if (horiAcc.Equals(float3.zero)) return;
        var time = Time.DeltaTime;

        Entities
        .WithName("ComsMove")
        .ForEach((ref PhysicsVelocity physicsVelocity, ref ComsData data, in Translation translation, in Rotation rotation, in PhysicsMass physicsMass) =>
        {
            // 垂直惯性力的添加通过修改全局
            // 垂直速度或位移小于阈值时代表物体不在空中
            if (math.abs(physicsVelocity.Linear.y) < threshold || translation.Value.y - data.previous_y < threshold)
            {
                // 当物体静止且水平惯性力小于最大静摩擦力时，直接不添加力，简化计算
                // 即当物体运动或者水平惯性力大于最大静摩擦力时，才添加水平惯性力到物体上
                // ma_{h}<μm(g-a_{v}) 表示水平惯性力小于最大静摩擦力
                if (horiAcc.Equals(float3.zero)) return;

                if (math.length(physicsVelocity.Linear.xz) >= threshold || math.length(horiAcc) >= math.abs(staticFriction * (gravity - vertiAcc)))
                {
                    physicsVelocity.ApplyLinearImpulse(physicsMass, -horiAcc / physicsMass.InverseMass * time);
                }
            }
            else
            {
                // 空气阻力 k = 1/2ρc_{d}A = 0.01f;ρ = 1.29;c_{d} = 0.8;A = 0.02
                // physicsVelocity.ApplyLinearImpulse(physicsMass, -math.normalize(physicsVelocity.Linear) * 0.01f * math.pow(math.length(physicsVelocity.Linear), 2) * time);
                physicsVelocity.ApplyLinearImpulse(physicsMass, -math.length(physicsVelocity.Linear) * 0.01f * physicsVelocity.Linear * time);
            }
            data.previous_y = translation.Value.y;
        }).ScheduleParallel();
    }
}
