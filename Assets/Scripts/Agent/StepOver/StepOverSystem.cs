using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class StepOverSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    // protected override void OnCreate() => buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    protected override void OnUpdate()
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var deltaTime = SystemAPI.Time.DeltaTime;
        // 让人物不摔倒，同时跨越地面的障碍物
        Entities.WithAll<Escaping>().WithReadOnly(physicsWorld).ForEach((ref LocalTransform localTransform, ref PhysicsGravityFactor physicsGravity, ref StepDurationData stepDurationData, in PhysicsVelocity velocity) =>
        {
            // 保持Agent不摔倒
            // rotation.Value = quaternion.Euler(0, 0, 0);
            // 不移动时不进行爬坡检测
            float3 vel = velocity.Linear;
            // 只考虑水平速度
            vel.y = 0;
            // 速度小于0.2m/s2时不进行爬坡检测
            if (math.lengthsq(vel) < 0.04) return;

            // 人物半径 0.25f
            float3 origin = localTransform.Position + (math.normalize(vel) * 0.27f);
            // origin.y += 1.1f;
            var bottom = origin.y - 0.9f;
            RaycastInput cast = new RaycastInput
            {
                Start = origin,
                End = origin + math.down() * origin.y,
                Filter = CollisionFilter.Default
            };
            physicsWorld.CastRay(cast, out RaycastHit hit);
            var deltaDis = hit.Position.y - bottom;
            if (deltaDis > 0 && deltaDis < 0.4f && localTransform.Position.y < 1.5f)
            {
                physicsGravity.Value = 0;
                localTransform.Position.y += deltaDis * 1.5f;
                stepDurationData.Value = 0.2f;
            }
            else if (stepDurationData.Value > 0)
            {
                stepDurationData.Value -= deltaTime;
                return;
            }
            else physicsGravity.Value = 1;

        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
