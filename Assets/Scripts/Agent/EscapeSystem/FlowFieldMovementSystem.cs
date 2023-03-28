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

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.Enabled = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();
        var deltaTime = SystemAPI.Time.DeltaTime;
        var cells = SystemAPI.GetSingletonBuffer<CellBuffer>(true).Reinterpret<CellData>().AsNativeArray();
        var des = SystemAPI.GetSingletonBuffer<DestinationBuffer>(true).Reinterpret<int>().AsNativeArray();

        if (setting.index == 0)
        {
            // Global FlowField
            state.Dependency = new GlobalFlowFieldJob
            {
                cells = cells,
                settingData = setting
            }.ScheduleParallel(state.Dependency);
        }
        else if (setting.index == 1)
        {// Basic SFM + Local FlowField
            state.Dependency = new BasicSFM_LocalFFJob
            {
                deltaTime = deltaTime,
                des = cells[des[0]].worldPos.xz,
                physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                cells = cells,
                settingData = setting
            }.ScheduleParallel(state.Dependency);
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
    void Execute(ref PhysicsVelocity velocity, in LocalTransform localTransform)
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
            var dir = math.normalizesafe(targetPos.xz - localTransform.Position.xz) + 0.75f * math.normalizesafe(targetDir / 4);

            // localTransform.Position.xz += dir2 * deltaTime * vel;
            velocity.Linear.xz = math.normalizesafe(dir) * vel;

        }
    }
}