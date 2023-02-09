using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(AgentSimulationSystemGroup)), UpdateAfter(typeof(AgentStateChangeSystem))]
[BurstCompile]
public partial struct RecordSystem : ISystem, ISystemStartStop
{
    private ComponentLookup<Escaping> escapingList;
    [BurstCompile]
    public void OnCreate(ref SystemState state) => state.Enabled = false;
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        escapingList = SystemAPI.GetComponentLookup<Escaping>(true);
        new InitialRecordDataJob().ScheduleParallel(state.Dependency).Complete();
    }
    [BurstCompile]
    public void OnStopRunning(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        escapingList.Update(ref state);
        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();

        new RecordAgentJob
        {
            elapsedTime = SystemAPI.GetSingleton<TimerData>().elapsedTime,
            escapingList = escapingList
            // recordList = recordList
        }.ScheduleParallel(state.Dependency).Complete();
    }
}

[WithAll(typeof(AgentMovementData), typeof(RecordData))]
[BurstCompile]
partial struct InitialRecordDataJob : IJobEntity
{
    void Execute(ref RecordData data, in LocalTransform localTransform)
    {
        data.lastPosition = localTransform.Position;
        data.escapedTime = 0;
        data.escapedLength = 0;
        data.escapeAveVel = 0;
        data.accumulatedY = 0;
    }
}

[BurstCompile]
[WithNone(typeof(Idle)), WithAll(typeof(AgentMovementData)),
WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
partial struct RecordAgentJob : IJobEntity
{
    [ReadOnly] public float elapsedTime;
    [NativeDisableParallelForRestriction]
    [ReadOnly] public ComponentLookup<Escaping> escapingList;
    void Execute(Entity e, ref RecordData data, in LocalTransform localTransform)
    {
        if (escapingList.IsComponentEnabled(e))
        {
            data.escapedLength += math.length(localTransform.Position.xz - data.lastPosition.xz);
            data.accumulatedY += math.abs(localTransform.Position.y - data.lastPosition.y);
            data.lastPosition = localTransform.Position;
        }
        else
        {
            if (data.escapedTime > 0) return;
            data.escapedTime = elapsedTime;
            data.escapeAveVel = data.escapedLength / elapsedTime;
        }
    }
}

// 虽然下面两个 Job 访问的是不同 Entity，但是目前的JobDependency似乎不是基于 Entity 的，而是基于组件类型，因此在同一系统中不能同时执行下述两个 Job，但在不同系统中同时执行似乎是可行的，因此将下述两个 Job 重写，合并为上方的单个 Job
// https://forum.unity.com/threads/running-different-systems-in-parallel-writing-to-the-same-componentdata.609667/
[BurstCompile]
[WithNone(typeof(Escaped), typeof(Idle)), WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct RecordEscapingAgentJob : IJobEntity
{
    void Execute(ref RecordData data, in LocalTransform localTransform)
    {
        data.escapedLength += math.length(localTransform.Position.xz - data.lastPosition.xz);
        data.accumulatedY += math.abs(localTransform.Position.y - data.lastPosition.y);
        data.lastPosition = localTransform.Position;
    }
}

[BurstCompile]
[WithNone(typeof(Idle), typeof(Escaping)), WithAll(typeof(AgentMovementData), typeof(Escaped)),
WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
partial struct RecordEscapedAgentJob : IJobEntity
{
    [ReadOnly] public float elapsedTime;
    void Execute(Entity e, ref RecordData data, in LocalTransform localTransform)
    {
        if (data.escapedTime > 0) return;
        data.escapedTime = elapsedTime;
        data.escapeAveVel = data.escapedLength / elapsedTime;
    }
}