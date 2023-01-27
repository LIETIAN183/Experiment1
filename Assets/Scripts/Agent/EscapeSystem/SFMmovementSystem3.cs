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
        // buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        this.Enabled = false;
    }
    protected override void OnStartRunning()
    {
        Entities.WithAll<AgentMovementData>().ForEach((ref AgentMovementData data, in LocalTransform localTransform) =>
        {
            data.originPosition = localTransform.Position;
        }).ScheduleParallel();
        this.CompleteDependency();
    }
    protected override void OnUpdate()
    {
        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = SystemAPI.Time.DeltaTime;
        // 用于物体检测
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        float2 SFMtarget = SystemAPI.GetSingleton<FlowFieldSettingData>().destination.xz;

        var accData = SystemAPI.GetSingleton<TimerData>();

        Entities.WithAll<Escaping>().WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, in AgentMovementData movementData, in LocalTransform localTransform, in PhysicsMass mass) =>
        {
            // 计算附近的障碍物与智能体
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
            physicsWorld.OverlapSphere(localTransform.Position, 1, ref outHits, CollisionFilter.Default);
            float2 interactionForce = 0;
            foreach (var hit in outHits)
            {
                if ((hit.Material.CustomTags & 0b_0001_1110) != 0)
                {
                    if (hit.Entity.Equals(entity)) continue;
                    var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                    interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f) * direction;
                }
            }

            var SFMDirection = math.normalizesafe(SFMtarget - localTransform.Position.xz);
            var desireSpeed = math.exp(-localTransform.Position.y + movementData.originPosition.y - math.length(accData.curAcc)) * movementData.stdVel;
            // var desireSpeed = movementData.stdVel;
            // desireSpeed = math.max(desireSpeed, 0);
            velocity.Linear.xz += ((SFMDirection * desireSpeed - velocity.Linear.xz) / 0.5f - accData.curAcc.xz + interactionForce * mass.InverseMass) * deltaTime;

            outHits.Dispose();
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}