using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;

// 仿真时间 0.1f 可行
// 基础社会力模型
[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
public partial class SFMmovementSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate()
    {
        // buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = SystemAPI.Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        float2 SFMtarget = SystemAPI.GetSingleton<FlowFieldSettingData>().destination.xz;

        Entities.WithAll<Escaping>().WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, in AgentMovementData movementData, in LocalTransform localTransform, in PhysicsMass mass) =>
        {
            // 计算附近的障碍物与智能体
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
            physicsWorld.OverlapSphere(localTransform.Position, 1, ref outHits, CollisionFilter.Default);
            float2 interactionForce = 0;
            foreach (var hit in outHits)
            {
                if ((hit.Material.CustomTags & 0b_1111_1111) != 0)
                {
                    if (hit.Entity.Equals(entity)) continue;
                    var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                    interactionForce += 2000 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                }
            }

            var SFMDirection = math.normalizesafe(SFMtarget - localTransform.Position.xz);
            velocity.Linear.xz += ((SFMDirection * movementData.stdVel - velocity.Linear.xz) / 0.5f + interactionForce * mass.InverseMass) * deltaTime;
            outHits.Dispose();
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
