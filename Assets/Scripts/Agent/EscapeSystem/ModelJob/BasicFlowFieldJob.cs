using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Jobs;

[BurstCompile]
public struct BasicCalculateCostJob : IJobParallelFor
{
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public float3 cellRadius;
    public NativeArray<CellData> cells;

    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        physicsWorld.OverlapBox(curCell.worldPos, quaternion.identity, cellRadius, ref outHits, Constants.ignorAgentGroundFilter);

        // costField 参数初始化
        curCell.massVariable = 0;
        curCell.maxHeight = 0;
        curCell.localCost = 1;
        curCell.integrationCost = Constants.T_i;
        curCell.fluidElementCount = 0;
        foreach (var hit in outHits)
        {
            // 墙壁等障碍物
            if ((hit.Material.CustomTags & 0b_0100_0000) != 0)
            {
                curCell.localCost = Constants.T_c;
                break;
            }
        }

        if (curCell.localCost == 1)
        {
            physicsWorld.OverlapBox(curCell.worldPos, quaternion.identity, cellRadius * 3, ref outHits, Constants.ignorAgentGroundFilter);
            foreach (var hit in outHits)
            {
                // 墙壁等障碍物
                if ((hit.Material.CustomTags & 0b_0100_0000) != 0)
                {
                    curCell.localCost = Constants.T_c / 2;
                    break;
                }
            }
        }


        cells[flatIndex] = curCell;
        outHits.Dispose();
    }
}

[BurstCompile]
public struct CalCulateIntegration_DijkstraJob : IJob
{
    public NativeArray<CellData> cells;
    [ReadOnly] public NativeArray<int> dests;
    [ReadOnly] public int2 gridSetSize;
    public void Execute()
    {
        NativeQueue<int> indicesToCheck = new NativeQueue<int>(Allocator.TempJob);

        foreach (var index in dests)
        {
            indicesToCheck.Enqueue(index);
        }

        while (indicesToCheck.Count > 0)
        {
            int cellFlatIndex = indicesToCheck.Dequeue();
            CellData curCellData = cells[cellFlatIndex];

            var neighborIndexList = FlowFieldUtility.Get8NeighborFlatIndices(curCellData.gridIndex, gridSetSize);
            foreach (int flatNeighborIndex in neighborIndexList)
            {
                CellData neighborCellData = cells[flatNeighborIndex];
                // 更新第一层障碍物的最佳方向
                if (neighborCellData.localCost >= Constants.T_c)
                {
                    continue;
                }
                // 更新 integrationCost
                float newIntegrationCost = neighborCellData.localCost + curCellData.integrationCost;

                if (newIntegrationCost < neighborCellData.integrationCost)
                {
                    neighborCellData.integrationCost = newIntegrationCost;
                    cells[flatNeighborIndex] = neighborCellData;
                    indicesToCheck.Enqueue(flatNeighborIndex);
                }
            }
            neighborIndexList.Dispose();
        }
        indicesToCheck.Dispose();
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct BasicFlowFieldJob : IJobEntity
{
    [ReadOnly] public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;
    void Execute(ref PhysicsVelocity velocity, in LocalTransform localTransform, in AgentMovementData movementData)
    {
        float2 globalGuidanceDir = float2.zero;
        float minValue = float.MaxValue;
        float2 targetDir = float2.zero;

        var index = FlowFieldUtility.GetCellFlatIndexFromWorldPos(localTransform.Position.xz, settingData.originPoint, settingData.gridSetSize, settingData.cellRadius * 2);

        foreach (var item in FlowFieldUtility.Get8NeighborFlatIndices(cells[index].gridIndex, settingData.gridSetSize))
        {
            if (cells[item].integrationCost <= minValue)
            {
                minValue = cells[item].integrationCost;
                targetDir = cells[item].gridIndex - cells[index].gridIndex;
            }
        }
        velocity.Linear.xz = math.normalizesafe(targetDir) * movementData.stdVel;
    }
}