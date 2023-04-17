using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;

[UpdateInGroup(typeof(AgentSimulationSystemGroup)), UpdateAfter(typeof(AgentMovementSystemGroup))]
[BurstCompile]
public partial struct AgentStateChangeSystem : ISystem
{
    private ComponentLookup<Idle> idleList;
    private ComponentLookup<Escaping> escapingList;
    private ComponentLookup<Escaped> escapedList;
    private EntityQuery idleQuery;

    private EntityQuery escapedQuery;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        idleList = SystemAPI.GetComponentLookup<Idle>();
        escapingList = SystemAPI.GetComponentLookup<Escaping>();
        escapedList = SystemAPI.GetComponentLookup<Escaped>();
        idleQuery = state.GetEntityQuery(ComponentType.ReadOnly<Idle>());
        state.Enabled = false;
        escapedQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Escaped>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(ref state);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (idleQuery.CalculateEntityCount() > 0)
        {
            idleList.Update(ref state);
            escapingList.Update(ref state);
            var timerData = SystemAPI.GetSingleton<TimerData>();
            new ActiveEscapeJob
            {
                idleList = idleList,
                escapingList = escapingList,
                PGAInms2 = timerData.curPGA * Constants.gravity,
                elapsedTime = timerData.elapsedTime
            }.ScheduleParallel(state.Dependency).Complete();
        }
        else
        {
            escapingList.Update(ref state);
            escapedList.Update(ref state);
            state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();

            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            new CheckArriveJob
            {
                parallelECB = ecb.AsParallelWriter(),
                escapingList = escapingList,
                escapedList = escapedList,
                dests = SystemAPI.GetSingletonBuffer<DestinationBuffer>(true).Reinterpret<int>().AsNativeArray(),
                cells = SystemAPI.GetSingletonBuffer<CellBuffer>(true).Reinterpret<CellData>().AsNativeArray()
            }.ScheduleParallel(state.Dependency).Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        // 若状态转换后，所有都撤离成功，则结束仿真
        var escaped = escapedQuery.CalculateEntityCount();
        var simulationSetting = SystemAPI.GetSingleton<SimConfigData>();
        if (!simulationSetting.performStatistics && escaped.Equals(SystemAPI.GetSingleton<SpawnerData>().desireCount))
        {
            SystemAPI.SetSingleton(new EndSeismicEvent { isActivate = true });
        }
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Idle)), WithNone(typeof(Escaping), typeof(Escaped))]
partial struct ActiveEscapeJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Idle> idleList;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Escaping> escapingList;
    [ReadOnly] public float PGAInms2;
    [ReadOnly] public float elapsedTime;
    void Execute(Entity e, in AgentMovementData data)
    {
        // 当时间超过行人的反应时间后，行人开始移动
        if (elapsedTime > data.reactionCofficient * 3.61f / PGAInms2)
        {
            idleList.SetComponentEnabled(e, false);
            escapingList.SetComponentEnabled(e, true);
        }
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping)), WithNone(typeof(Idle), typeof(Escaped))]
partial struct CheckArriveJob : IJobEntity
{
    [ReadOnly] public NativeArray<int> dests;
    [NativeDisableParallelForRestriction]
    [ReadOnly] public NativeArray<CellData> cells;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Escaping> escapingList;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Escaped> escapedList;

    public EntityCommandBuffer.ParallelWriter parallelECB;

    void Execute(Entity e, [EntityIndexInQuery] int entityIndex, in LocalTransform localTransform)
    {
        float minDisSquare = float.MaxValue, temp;
        foreach (var index in dests)
        {
            temp = math.lengthsq(cells[index].worldPos.xz - localTransform.Position.xz);
            if (temp < minDisSquare)
            {
                minDisSquare = temp;
            }
        }
        if (minDisSquare < 0.25f)
        {
            escapingList.SetComponentEnabled(e, false);
            escapedList.SetComponentEnabled(e, true);
            parallelECB.AddComponent<Disabled>(entityIndex, e);
        }
    }
}