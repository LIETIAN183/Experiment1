using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;

public class ConstraintsSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    protected override void OnCreate() => buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    protected override void OnUpdate()
    {
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;
        // 让人物不摔倒，同时跨越地面的障碍物
        Entities.WithReadOnly(physicsWorld).ForEach((ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, ref PhysicsGravityFactor physicsGravity, in AgentMovementData movementData) =>
        {
            // 保持Agent不摔倒
            // rotation.Value = quaternion.Euler(0, 0, 0);

            // 判断是否处于 Escape 状态
            if (movementData.state != AgentState.Escape)
            {
                physicsGravity.Value = 1;
                return;
            }
            // 不移动时不进行爬坡检测
            float2 vel = velocity.Linear.xz;
            //不移动时不进行爬坡检测
            if (vel.Equals(float2.zero)) return;
            // 人物半径 0.25f
            float3 origin = translation.Value.addFloat2(math.normalize(vel) * 0.26f);
            // origin.y += 1.1f;
            var bottom = origin.y - 1;
            RaycastInput cast = new RaycastInput
            {
                Start = origin,
                End = origin + math.down() * origin.y,
                Filter = CollisionFilter.Default
            };
            physicsWorld.CastRay(cast, out RaycastHit hit);
            var deltaDis = hit.Position.y - bottom;
            if (deltaDis > 0 && deltaDis < 0.4f && translation.Value.y < 1.5f)
            {
                physicsGravity.Value = 0;
                translation.Value.y += deltaDis * 1.2f;
            }
            else physicsGravity.Value = 1;

        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
