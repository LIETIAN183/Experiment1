using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateInGroup(typeof(AgentMovementSystemGroup))]
[BurstCompile]
public partial struct FlowFieldMovementSystem : ISystem
{
    private EntityQuery escapedQuery;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        escapedQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Escaped>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(ref state);
        state.Enabled = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        state.Dependency = new FlowFieldMovementJob
        {
            cells = SystemAPI.GetSingletonBuffer<CellBuffer>(true).Reinterpret<CellData>().AsNativeArray(),
            settingData = SystemAPI.GetSingleton<FlowFieldSettingData>(),
            deltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel(state.Dependency);

        var escaped = escapedQuery.CalculateEntityCount();
        var simulationSetting = SystemAPI.GetSingleton<SimConfigData>();
        if (!simulationSetting.performStatistics && escaped.Equals(SystemAPI.GetSingleton<SpawnerData>().desireCount))
        {
            SystemAPI.SetSingleton(new EndSeismicEvent { isActivate = true });
        }
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct FlowFieldMovementJob : IJobEntity
{
    [ReadOnly] public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;
    [ReadOnly] public float deltaTime;
    void Execute(ref LocalTransform localTransform)
    {
        int2 localCellIndex = FlowFieldUtility.GetCellIndexFromWorldPos(localTransform.Position, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
        if (!localCellIndex.Equals(Constants.notInGridSet))
        {
            int flatLocalCellIndex = FlowFieldUtility.ToFlatIndex(localCellIndex, settingData.gridSetSize.y);
            var targetDir = math.normalizesafe(cells[flatLocalCellIndex].bestDir);
            localTransform.Position.xz += targetDir * 2 * deltaTime;
        }
    }
}