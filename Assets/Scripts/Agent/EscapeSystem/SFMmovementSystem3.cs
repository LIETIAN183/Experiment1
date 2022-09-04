using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;

// 仿真时间 0.1f 可行
// 我们的社会力模型，无流场
[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
public partial class SFMmovementSystem3 : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        this.Enabled = false;
    }
    protected override void OnStartRunning()
    {
        Entities.WithAll<AgentMovementData>().ForEach((ref AgentMovementData data, in Translation translation) =>
        {
            data.originPosition = translation.Value;
        }).ScheduleParallel();
        this.CompleteDependency();
    }
    protected override void OnUpdate()
    {
        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        float2 SFMtarget = GetSingleton<FlowFieldSettingData>().destination.xz;

        var accData = GetSingleton<AccTimerData>();

        Entities.WithAll<Escaping>().WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, in AgentMovementData movementData, in Translation translation, in PhysicsMass mass) =>
        {
            // 计算附近的障碍物与智能体
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
            physicsWorld.OverlapSphere(translation.Value, 1, ref outHits, CollisionFilter.Default);
            float2 interactionForce = 0;
            foreach (var hit in outHits)
            {
                if ((hit.Material.CustomTags & 0b_0001_1110) != 0)
                {
                    if (hit.Entity.Equals(entity)) continue;
                    var direction = math.normalizesafe(translation.Value.xz - hit.Position.xz);
                    interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                }
            }

            var SFMDirection = math.normalizesafe(SFMtarget - translation.Value.xz);
            var desireSpeed = math.exp(-translation.Value.y + movementData.originPosition.y - math.length(accData.acc)) * movementData.stdVel;
            // var desireSpeed = movementData.stdVel;
            // desireSpeed = math.max(desireSpeed, 0);
            velocity.Linear.xz += ((SFMDirection * desireSpeed - velocity.Linear.xz) / 0.5f - accData.acc.xz + interactionForce * mass.InverseMass) * deltaTime;

            outHits.Dispose();
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}