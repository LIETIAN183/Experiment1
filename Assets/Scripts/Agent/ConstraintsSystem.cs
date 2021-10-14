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

        // RaycastHit
        // physicsWorld.CastRay(cast, out closesteHit);

        Entities.WithoutBurst().WithReadOnly(physicsWorld).WithAll<AgentMovementData>().ForEach((ref Translation translation, ref Rotation rotation, in PhysicsVelocity velocity) =>
        {
            rotation.Value = quaternion.Euler(0, 0, 0);
            // 不移动时不进行爬坡检测
            if (velocity.Linear.Equals(float3.zero)) return;
            var vel = velocity.Linear;
            vel.y = 0;
            float3 origin = translation.Value + math.normalize(vel) * 0.35f;
            RaycastInput cast = new RaycastInput
            {
                Start = origin,
                End = origin + math.down(),
                Filter = CollisionFilter.Default
            };
            physicsWorld.CastRay(cast, out RaycastHit hit);
            // Debug.Log(hit.Position);
            if (hit.Position.y < 0.1f)
            {
                translation.Value.y += hit.Position.y * 1.3f;
            }
            ;// - hit.Position.y;
             // hit.Position
             // TODO:

        }).ScheduleParallel();

        this.CompleteDependency();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Entities.WithAll<AgentMovementData>().ForEach((in Translation translation, in PhysicsVelocity velocity) =>
        {
            if (velocity.Linear.Equals(float3.zero)) return;
            Gizmos.DrawRay(translation.Value + math.normalize(velocity.Linear) * 0.4f, math.down());
        }).Run();
    }
}
