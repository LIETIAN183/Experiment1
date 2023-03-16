using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

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
    void Execute(ref PhysicsVelocity velocity, ref LocalTransform localTransform)
    {
        int vel = 4;

        var pos1 = localTransform.Position + new float3(0.2f, 0, 0.2f);
        var pos2 = localTransform.Position + new float3(-0.2f, 0, 0.2f);
        var pos3 = localTransform.Position + new float3(0.2f, 0, -0.2f);
        var pos4 = localTransform.Position + new float3(-0.2f, 0, -0.2f);
        int localCellIndex = FlowFieldUtility.GetCellFlatIndexFromWorldPos(localTransform.Position, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);

        var flatIndex1 = FlowFieldUtility.GetCellFlatIndexFromWorldPos(pos1, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
        var flatIndex2 = FlowFieldUtility.GetCellFlatIndexFromWorldPos(pos2, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
        var flatIndex3 = FlowFieldUtility.GetCellFlatIndexFromWorldPos(pos3, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
        var flatIndex4 = FlowFieldUtility.GetCellFlatIndexFromWorldPos(pos4, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);
        float2 targetDir = float2.zero;

        if (settingData.agentIndex == 0)
        {
            int count = 0;
            if (flatIndex1 > 0)
            {
                var delta = math.abs(cells[flatIndex1].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex1].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex1].globalDir;
                count++;
            }
            if (flatIndex2 > 0)
            {
                var delta = math.abs(cells[flatIndex2].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex2].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex2].globalDir;
                count++;
            }
            if (flatIndex3 > 0)
            {
                var delta = math.abs(cells[flatIndex3].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex3].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex3].globalDir;
                count++;
            }
            if (flatIndex4 > 0)
            {
                var delta = math.abs(cells[flatIndex4].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex4].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex4].globalDir;
                count++;
            }
            // localTransform.Position.xz += math.normalizesafe(targetDir) * deltaTime * vel;
            velocity.Linear.xz = math.normalizesafe(targetDir / count) * vel;
        }
        else if (settingData.agentIndex == 1)
        {
            int count = 0;
            if (flatIndex1 > 0)
            {
                var delta = math.abs(cells[flatIndex1].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex1].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex1].localDir;
                count++;
            }
            if (flatIndex2 > 0)
            {
                var delta = math.abs(cells[flatIndex2].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex2].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex2].localDir;
                count++;
            }
            if (flatIndex3 > 0)
            {
                var delta = math.abs(cells[flatIndex3].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex3].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex3].localDir;
                count++;
            }
            if (flatIndex4 > 0)
            {
                var delta = math.abs(cells[flatIndex4].worldPos.x - localTransform.Position.x) + math.abs(cells[flatIndex4].worldPos.z - localTransform.Position.z);
                targetDir += (1 - delta) * cells[flatIndex4].localDir;
                count++;
            }
            var targetPos = new float3(8.75f, 1f, -1.25f);
            var dir = math.normalize(targetPos.xz - localTransform.Position.xz) + 0.9f * targetDir / count;

            // localTransform.Position.xz += dir2 * deltaTime * vel;
            velocity.Linear.xz = math.normalizesafe(dir) * vel;

        }
        else if (settingData.agentIndex == 2)
        {
            // localTransform.Position.xz += math.normalizesafe(cells[localCellIndex].globalDir) * deltaTime * vel;
            velocity.Linear.xz = math.normalizesafe(cells[localCellIndex].globalDir) * vel;
        }
        else if (settingData.agentIndex == 3)
        {
            var targetPos = new float3(8.75f, 1f, -1.25f);
            var dir = math.normalize(targetPos.xz - localTransform.Position.xz) + 0.9f * math.normalizesafe(cells[localCellIndex].localDir);
            // localTransform.Position.xz += dir2 * deltaTime * vel;
            velocity.Linear.xz = math.normalizesafe(dir) * vel;
        }

        // velocity.Linear.xz = new float2(1, 0) + 0.5f * math.normalizesafe(targetDir);




        // if (!localCellIndex.Equals(Constants.notInGridSet))
        // {
        //     int flatLocalCellIndex = FlowFieldUtility.ToFlatIndex(localCellIndex, settingData.gridSetSize.y);
        //     var targetDir = math.normalizesafe(cells[flatLocalCellIndex].bestDir);

        //     velocity.Linear.xz = targetDir * 2;
        //     // localTransform.Position.xz += targetDir * 2 * deltaTime;
        // }
    }
}