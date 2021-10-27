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
        Entities.WithoutBurst().WithReadOnly(physicsWorld).WithAll<AgentMovementData>().ForEach((ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, ref PhysicsGravityFactor physicsGravity, in AgentMovementData movementData) =>
        {
            // 保持Agent不摔倒
            rotation.Value = quaternion.Euler(0, 0, 0);

            // 判断是否处于 Escape 状态
            if (movementData.state == AgentState.NotActive)
            {
                physicsGravity.Value = 1;
                return;
            }
            // 不移动时不进行爬坡检测
            // TODO: 约束条件更精确一些
            // if (velocity.Linear.Equals(float3.zero)) return;
            var vel = velocity.Linear;
            vel.y = 0;
            if (vel.Equals(float3.zero)) return;
            float3 origin = translation.Value + math.normalize(vel) * 0.26f;
            RaycastInput cast = new RaycastInput
            {
                Start = origin,
                End = origin + math.down(),
                Filter = CollisionFilter.Default
            };
            physicsWorld.CastRay(cast, out RaycastHit hit);
            var delta = 1 - translation.Value.y + hit.Position.y;
            if (delta >= 0f && delta < 0.4f)
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
}
