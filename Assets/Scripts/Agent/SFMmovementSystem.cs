using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;

// 仿真时间 0.1f 可行
[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
[DisableAutoCreation]
public class SFMmovementSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate() => buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    protected override void OnUpdate()
    {
        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        float2 SFMtarget = GetSingleton<FlowFieldSettingData>().destination.xz;

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
                    if (hit.Material.CustomTags.Equals(2) || hit.Material.CustomTags.Equals(4))//00000010 障碍物 00000100 智能体
                    {
                        if (hit.Entity.Equals(entity)) continue;
                        var direction = math.normalizesafe(translation.Value.xz - hit.Position.xz);
                        interactionForce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                    }
                }

                var SFMDirection = math.normalizesafe(SFMtarget - translation.Value.xz);
                velocity.Linear.xz += ((SFMDirection * movementData.desireSpeed - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass) * deltaTime;
                outHits.Dispose();
            }

        }).ScheduleParallel();

        this.CompleteDependency();
    }
}