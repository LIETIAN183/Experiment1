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
        // TODO：时间限制为了物品掉落先
        if (idleQuery.CalculateEntityCount() > 0)
        {
            if (SystemAPI.GetSingleton<TimerData>().elapsedTime > 5 && SystemAPI.GetSingleton<SimConfigData>().simEnvironment)
            {
                idleList.Update(ref state);
                escapingList.Update(ref state);
                new ActiveEscapeJob
                {
                    idleList = idleList,
                    escapingList = escapingList
                }.ScheduleParallel(state.Dependency).Complete();
            }
            // if(!SystemAPI.GetSingleton<SimConfigData>().simEnvironment){
            //     idleList.Update(ref state);
            //     escapingList.Update(ref state);
            //     new ActiveEscapeJob
            //     {
            //         idleList = idleList,
            //         escapingList = escapingList
            //     }.ScheduleParallel(state.Dependency).Complete();
            // }
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
    void Execute(Entity e)
    {
        idleList.SetComponentEnabled(e, false);
        escapingList.SetComponentEnabled(e, true);
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
        if (minDisSquare < 1f)
        {
            escapingList.SetComponentEnabled(e, false);
            escapedList.SetComponentEnabled(e, true);
            parallelECB.AddComponent<Disabled>(entityIndex, e);
        }
    }
}