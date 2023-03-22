using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public struct CalculateGlobalFlowFieldJob_NotDestGrid_8Neighbor : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public int2 gridSetSize;
    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        float2 dir = float2.zero;
        float lowerestInt = float.MaxValue;
        var flatNeighborIndexList = FlowFieldUtility.Get8NeighborFlatIndices(curCell.gridIndex, gridSetSize);
        foreach (int flatNeighborIndex in flatNeighborIndexList)
        {
            CellData neighborCell = cells[flatNeighborIndex];
            if (lowerestInt > neighborCell.integrationCost)
            {
                lowerestInt = neighborCell.integrationCost;
                dir = neighborCell.gridIndex - curCell.gridIndex;
            }
        }
        flatNeighborIndexList.Dispose();

        curCell.globalDir = dir;
        cells[flatIndex] = curCell;
    }
}

[BurstCompile]
public struct CalculateLocalFlowFieldJob_NotDestGrid_8Neighbor : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public int2 gridSetSize;
    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        float2 localDir = float2.zero;
        float minLocalCost = float.MaxValue;
        var flatNeighborIndexList = FlowFieldUtility.Get8NeighborFlatIndices(curCell.gridIndex, gridSetSize);
        foreach (int flatNeighborIndex in flatNeighborIndexList)
        {
            CellData neighborCell = cells[flatNeighborIndex];
            if (neighborCell.localCost < minLocalCost)
            {
                localDir = neighborCell.gridIndex - curCell.gridIndex;
                minLocalCost = neighborCell.localCost;
            }
        }
        if (curCell.localCost == 1 && minLocalCost == 1)
        {
            localDir = float2.zero;
        }
        flatNeighborIndexList.Dispose();

        curCell.localDir = localDir;
        cells[flatIndex] = curCell;
    }
}