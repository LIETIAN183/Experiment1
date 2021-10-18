using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;

// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class ConstraintsSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }
    protected override void OnUpdate()
    {

        var physicsWorld = buildPhysicsWorld.PhysicsWorld;
        // 让人物不摔倒，同时跨越地面的障碍物
        Entities.WithoutBurst().WithReadOnly(physicsWorld).WithAll<AgentMovementData>().ForEach((ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, ref PhysicsGravityFactor physicsGravity) =>
        {
            rotation.Value = quaternion.Euler(0, 0, 0);
            // 不移动时不进行爬坡检测
            if (velocity.Linear.Equals(float3.zero)) return;
            var vel = velocity.Linear;
            vel.y = 0;
            float3 origin = translation.Value + math.normalize(vel) * 0.26f;
            RaycastInput cast = new RaycastInput
            {
                Start = origin,
                End = origin + math.down(),
                Filter = CollisionFilter.Default
            };
            physicsWorld.CastRay(cast, out RaycastHit hit);
            // Debug.Log(hit.Position);
            var delta = 1 - translation.Value.y + hit.Position.y;
            if (delta >= 0f && delta <= 0.5f)
            {
                physicsGravity.Value = 0;
                translation.Value.y += delta * 1.2f;
            }
            else
            {
                physicsGravity.Value = 1;
            }

        }).ScheduleParallel();

        this.CompleteDependency();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Entities.WithAll<AgentMovementData>().ForEach((in Translation translation, in PhysicsVelocity velocity) =>
        {
            if (velocity.Linear.Equals(float3.zero)) return;
            Gizmos.DrawRay(translation.Value + math.normalize(velocity.Linear) * 0.35f, math.down());
        }).Run();
    }
}
