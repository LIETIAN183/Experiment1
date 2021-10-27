using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Physics.Extensions;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ComsMotionSystem))]
[DisableAutoCreation]
public class AgentInteractionSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }
    protected override void OnUpdate()
    {
        float time = Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        Entities.WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, in AgentMovementData movementData, in Translation translation, in PhysicsMass mass) =>
        {
            if (movementData.state == AgentState.Escape)
            {
                // 计算附近的障碍物与智能体
                NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
                physicsWorld.OverlapSphere(translation.Value, 1, ref outHits, CollisionFilter.Default);
                float3 interactionForce = 0;
                foreach (var hit in outHits)
                {
                    if (hit.Material.CustomTags.Equals(2) || hit.Material.CustomTags.Equals(4))//00000010 障碍物
                    {
                        if (hit.Entity.Equals(entity)) continue;
                        var direction = translation.Value - hit.Position;
                        direction.y = 0;
                        direction = math.normalize(direction);
                        interactionForce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                    }
                }
                velocity.Linear += interactionForce * mass.InverseMass * time;
                // velocity.ApplyLinearImpulse(mass, interactionForce * time);
                outHits.Dispose();
            }
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
