using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Burst;

// TODO: 添加货架的排斥力
[UpdateInGroup(typeof(AgentMovementSystemGroup))]
// [DisableAutoCreation]
public partial class AgentMovementSystem : SystemBase
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
        DynamicBuffer<CellData> cellBuffer = SystemAPI.GetBuffer<CellBuffer>(SystemAPI.GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();
        if (cellBuffer.Length == 0) return;
        var settingData = SystemAPI.GetSingleton<FlowFieldSettingData>();

        // 用于计算最终的加速度，作为时间尺度
        float deltaTime = SystemAPI.Time.DeltaTime;
        // 用于物体检测
        // var physicsWorld = buildPhysicsWorld.PhysicsWorld;
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        var accData = SystemAPI.GetSingleton<TimerData>();

        float2 SFMtarget = SystemAPI.GetSingleton<FlowFieldSettingData>().destination.xz;

        Entities.WithAll<Escaping>().WithReadOnly(cellBuffer).WithReadOnly(physicsWorld).ForEach((Entity entity, ref PhysicsVelocity velocity, ref AgentMovementData movementData, in LocalTransform localTransform, in PhysicsMass mass) =>
        {
            // 获得当前所在位置的网格 Index
            int2 localCellIndex = FlowFieldUtility.GetCellIndexFromWorldPos(localTransform.Position, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
            // 获得当前的期望方向
            int flatLocalCellIndex = FlowFieldUtility.ToFlatIndex(localCellIndex, settingData.gridSetSize.y);

            // var SFMDirection = math.normalizesafe(SFMtarget - translation.Value.xz);
            float2 desireDirection = math.normalizesafe(cellBuffer[flatLocalCellIndex].bestDir);
            if (localTransform.Position.z > 5 || localTransform.Position.x < -5)
            {
                int2 neighberIndex = FlowFieldUtility.GetIndexAtRelativePosition(localCellIndex, (int2)cellBuffer[flatLocalCellIndex].bestDir, settingData.gridSetSize);
                int flatNeigborIndex = FlowFieldUtility.ToFlatIndex(neighberIndex, settingData.gridSetSize.y);
                if (cellBuffer[flatNeigborIndex].cost == 1)
                {
                    desireDirection = math.normalizesafe(SFMtarget - localTransform.Position.xz);
                }
            }
            // if (cellBuffer[flatLocalCellIndex].cost == 1 && Vector2.Angle(SFMDirection, desireDirection) < 50)
            // {
            //     desireDirection = SFMDirection;
            // }

            float2 interactionForce = 0;
            // pga 超过2没必要计算排斥力了
            if (accData.curPGA < 3)
            {
                // 计算和其他智能体的排斥力
                NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
                physicsWorld.OverlapSphere(localTransform.Position, 1, ref outHits, CollisionFilter.Default);
                foreach (var hit in outHits)
                {
                    if ((hit.Material.CustomTags & 0b_0001_0000) != 0)
                    {
                        if (hit.Entity.Equals(entity)) continue;
                        var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                        interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 0.08f - accData.curPGA) * direction;
                    }
                    // 和其他固定障碍物间的排斥力
                    if ((hit.Material.CustomTags & 0b_0000_1100) != 0)
                    {
                        if (hit.Entity.Equals(entity)) continue;
                        var direction = math.normalizesafe(localTransform.Position.xz - hit.Position.xz);
                        interactionForce += 20 * math.exp((0.25f - math.abs(hit.Fraction)) / 1f) * direction;
                    }
                }

                outHits.Dispose();
            }
            var desireSpeed = math.exp(-localTransform.Position.y + movementData.originPosition.y - math.length(accData.curAcc)) * movementData.stdVel;// originPosition.y 取代 1.05f
            movementData.desireSpeed = desireSpeed;
            movementData.curSpeed = math.length(velocity.Linear.xz);

            velocity.Linear.xz += ((desireDirection * desireSpeed - velocity.Linear.xz) / 0.5f - accData.curAcc.xz + interactionForce * mass.InverseMass) * deltaTime;
            movementData.nextSpeed = math.length(velocity.Linear.xz);

        }).ScheduleParallel();

        this.CompleteDependency();
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData))]
partial struct AgentBackupJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter parallelECB;
    void Execute(Entity e, [EntityIndexInQuery] int index, in LocalTransform localTransform)
    {
        parallelECB.AddComponent<OriginPos_RotInfo>(index, e, new OriginPos_RotInfo { orgPos = localTransform.Position, orgRot = localTransform.Rotation });
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(OriginPos_RotInfo)), WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
partial struct AgentRecoverJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Idle> idleList;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Escaped> escapedList;
    public EntityCommandBuffer.ParallelWriter parallelECB;
    void Execute(Entity e, [EntityIndexInQuery] int index, ref LocalTransform localTransform, ref PhysicsVelocity velocity, in OriginPos_RotInfo backup)
    {
        localTransform.Position = backup.orgPos;
        localTransform.Rotation = backup.orgRot;
        velocity.Linear = velocity.Angular = float3.zero;

        idleList.SetComponentEnabled(e, true);
        escapedList.SetComponentEnabled(e, false);
        parallelECB.RemoveComponent<Disabled>(index, e);
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(OriginPos_RotInfo)), WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
partial struct FlowFieldAgentRecoverJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Idle> idleList;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Escaped> escapedList;
    public EntityCommandBuffer.ParallelWriter parallelECB;
    void Execute(Entity e, [EntityIndexInQuery] int index, ref LocalTransform localTransform, in OriginPos_RotInfo backup)
    {
        localTransform.Position = backup.orgPos;
        localTransform.Rotation = backup.orgRot;

        idleList.SetComponentEnabled(e, true);
        escapedList.SetComponentEnabled(e, false);
        parallelECB.RemoveComponent<Disabled>(index, e);
    }
}