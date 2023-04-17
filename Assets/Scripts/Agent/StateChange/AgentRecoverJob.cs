using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Burst;

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
[WithAll(typeof(AgentMovementData), typeof(OriginPos_RotInfo), typeof(Escaped)), WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
partial struct AgentRecoverJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Idle> idleList;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Escaped> escapedList;
    public EntityCommandBuffer.ParallelWriter parallelECB;
    void Execute(Entity e, [EntityIndexInQuery] int index, ref LocalTransform localTransform, ref PhysicsVelocity velocity, ref AgentMovementData data, in OriginPos_RotInfo backup)
    {
        data.SeeExit = false;
        data.lastSelfDir = float2.zero;

        localTransform.Position = backup.orgPos;
        localTransform.Rotation = backup.orgRot;
        velocity.Linear = velocity.Angular = float3.zero;

        idleList.SetComponentEnabled(e, true);
        escapedList.SetComponentEnabled(e, false);
        parallelECB.RemoveComponent<Disabled>(index, e);
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(OriginPos_RotInfo), typeof(Escaped)), WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
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