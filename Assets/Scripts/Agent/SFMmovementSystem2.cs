using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;

// 仿真时间 0.1f 可行
[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
public class SFMmovementSystem2 : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        float2 SFMtarget = GetSingleton<FlowFieldSettingData>().destination.xz;

        var groundAcc = GetSingleton<AccTimerData>().acc.xz;

        Entities.WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, in AgentMovementData movementData, in Translation translation, in PhysicsMass mass) =>
        {
            if (movementData.state == AgentState.Escape)
            {
                // 计算附近的障碍物与智能体
                NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
                physicsWorld.OverlapSphere(translation.Value, 1, ref outHits, CollisionFilter.Default);
                float2 interactionForce = 0;
                foreach (var hit in outHits)
                {
                    if ((hit.Material.CustomTags & 0b_0000_1110) != 0)
                    {
                        var direction = math.normalizesafe(translation.Value.xz - hit.Position.xz);
                        interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.3f) * direction;
                    }
                    else if (((hit.Material.CustomTags & 0b_0001_0000) != 0))
                    {
                        if (hit.Entity.Equals(entity)) continue;
                        var direction = math.normalizesafe(translation.Value.xz - hit.Position.xz);
                        interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.5f) * direction;
                    }
                }

                var SFMDirection = math.normalizesafe(SFMtarget - translation.Value.xz);
                var acc = (SFMDirection * movementData.stdVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass;
                var flag1 = math.abs(acc.x) > math.abs(groundAcc.x);
                var flag2 = math.abs(acc.y) > math.abs(groundAcc.y);
                if (flag1 && flag2)
                {
                    velocity.Linear.xz += ((SFMDirection * movementData.stdVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass) * deltaTime;
                }
                else if (!flag1 && !flag2)
                {
                    velocity.Linear.xz = 0;
                }
                else if (!flag1 && flag2)
                {
                    velocity.Linear.x = 0;
                    velocity.Linear.z += ((SFMDirection * movementData.stdVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass).y * deltaTime;
                }
                else
                {
                    velocity.Linear.z = 0;
                    velocity.Linear.x += ((SFMDirection * movementData.stdVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass).x * deltaTime;
                }

                outHits.Dispose();
            }

        }).ScheduleParallel();

        this.CompleteDependency();
    }
}