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
public struct CalculateGlobalFlowFieldJob_NotDestGrid : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public float pgaInms2;
    [ReadOnly] public FlowFieldSettingData settingData;
    public void Execute(int flatIndex)
    {
        var gridSetSize = settingData.gridSetSize;
        int flatDestinationIndex = FlowFieldUtility.GetCellFlatIndexFromWorldPos(settingData.destination, settingData.originPoint, gridSetSize, settingData.cellRadius * 2);
        CellData destinationCell = cells[flatDestinationIndex];
        var targetPos = destinationCell.worldPos;

        var curCell = cells[flatIndex];
        float2 lowerDir = float2.zero, upperDir = float2.zero, dir;
        float diff;
        var flatNeighborIndexList = FlowFieldUtility.Get8NeighborFlatIndices(curCell.gridIndex, gridSetSize);
        foreach (int flatNeighborIndex in flatNeighborIndexList)
        {
            CellData neighborCell = cells[flatNeighborIndex];
            diff = curCell.integrationCost - neighborCell.integrationCost;
            if (diff == 0) continue;
            dir = neighborCell.gridIndex - curCell.gridIndex;
            if (diff > 0)
            {
                lowerDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
            }
            else
            {
                upperDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
            }
        }
        flatNeighborIndexList.Dispose();

        curCell.globalDir = math.normalizesafe(lowerDir) + Constants.c_avoid * math.exp(-pgaInms2) * math.normalizesafe(upperDir);
        // curCell.bestDir = math.normalizesafe(lowerDir);
        // curCell.targetDir = math.normalizesafe(targetPos.xz - curCell.worldPos.xz);
        // curCell.debugField = UnityEngine.Vector2.Angle(curCell.globalBestDir, curCell.targetDir);
        cells[flatIndex] = curCell;
    }
}

[BurstCompile]
public struct CalculateGlobalFlowFieldJob_DestGrid : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public NativeArray<int> dests;
    public void Execute(int flatIndex)
    {
        var curCell = cells[dests[flatIndex]];
        curCell.globalDir = float2.zero;
        cells[dests[flatIndex]] = curCell;
    }
}

[BurstCompile]
public struct CalculateLocalFlowFieldJob_NotDestGrid : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public float pgaInms2;
    [ReadOnly] public int2 gridSetSize;
    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        float2 localDir = float2.zero;
        float minLocalCost = float.MaxValue;
        float2 lowerDir = float2.zero, upperDir = float2.zero, dir;
        float diff;
        var flatNeighborIndexList = FlowFieldUtility.Get8NeighborFlatIndices(curCell.gridIndex, gridSetSize);
        foreach (int flatNeighborIndex in flatNeighborIndexList)
        {
            CellData neighborCell = cells[flatNeighborIndex];
            if (neighborCell.localCost > 1 && neighborCell.localCost < minLocalCost)
            {
                localDir = neighborCell.gridIndex - curCell.gridIndex;
                minLocalCost = neighborCell.localCost;
            }

            diff = curCell.localCost - neighborCell.localCost;
            if (diff == 0) continue;
            dir = neighborCell.gridIndex - curCell.gridIndex;
            if (diff > 0)
            {
                lowerDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
            }
            else
            {
                upperDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
            }
        }
        flatNeighborIndexList.Dispose();

        curCell.localDir = math.normalizesafe(lowerDir) + Constants.c_avoid * math.exp(-pgaInms2) * math.normalizesafe(upperDir);
        // curCell.localDir = math.normalizesafe(lowerDir);
        // curCell.localDir = localDir;
        cells[flatIndex] = curCell;
    }
}

/// <summary>
/// 计算局部流场时，行人的影响结不考虑
/// </summary>
[BurstCompile]
public struct CalculateLocalFlowFieldJob_DestGrid : IJob
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public NativeArray<int> dests;
    [ReadOnly] public float pgaInms2, gridVolume;
    [ReadOnly] public int2 gridSetSize;

    public void Execute()
    {
        float2 curLowerDir, curUpperDir, nbrLowerDir, nbrUpperDir, dir;
        float diff;
        foreach (var curIndex in dests)
        {
            curLowerDir = float2.zero;
            curUpperDir = float2.zero;
            // 计算目标网格的本地指导方向
            var curCell = cells[curIndex];
            curCell.localDir = float2.zero;
            // 对于不可行网格，需要额外判断，不能直接计算
            float curLocalCost;
            if (curCell.localCost >= Constants.T_c && curCell.maxHeight == 0)
            {
                curLocalCost = Constants.T_c;
            }
            else
            {
                curLocalCost = (curCell.massVariable + Constants.c2_fluid * curCell.fluidElementCount * 0.0083f) / gridVolume + math.exp(curCell.maxHeight) + curCell.maxHeight * Constants.c3;
            }
            NativeList<int> nbrList = FlowFieldUtility.Get8NeighborFlatIndices(curCell.gridIndex, gridSetSize);
            for (int i = 0; i < nbrList.Length; ++i)
            {
                var nbrFlatIndex = nbrList[i];
                CellData nbrCell = cells[nbrFlatIndex];
                float nbrLocalCost;
                if (nbrCell.localCost >= Constants.T_c && nbrCell.maxHeight == 0)
                {
                    nbrLocalCost = Constants.T_c;
                }
                else
                {
                    nbrLocalCost = (nbrCell.massVariable + Constants.c2_fluid * nbrCell.fluidElementCount * 0.0083f) / gridVolume + math.exp(nbrCell.maxHeight) + nbrCell.maxHeight * Constants.c3;
                }
                //计算邻接网格的本地指导方向
                float2 lowerDir2 = float2.zero, upperDir2 = float2.zero;
                nbrLowerDir = float2.zero;
                nbrUpperDir = float2.zero;
                var n2nList = FlowFieldUtility.Get8NeighborFlatIndices(nbrCell.gridIndex, gridSetSize);
                foreach (int n2nFlatIndex in n2nList)
                {
                    CellData n2nCell = cells[n2nFlatIndex];
                    float n2nLocalCost;
                    if (n2nCell.localCost >= Constants.T_c && n2nCell.maxHeight == 0)
                    {
                        n2nLocalCost = Constants.T_c;
                    }
                    else
                    {
                        n2nLocalCost = (n2nCell.massVariable + Constants.c2_fluid * n2nCell.fluidElementCount * 0.0083f) / gridVolume + math.exp(n2nCell.maxHeight) + n2nCell.maxHeight * Constants.c3;
                    }
                    diff = nbrLocalCost - n2nLocalCost;
                    if (diff == 0) continue;
                    dir = n2nCell.gridIndex - nbrCell.gridIndex;
                    if (diff > 0)
                    {
                        nbrLowerDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
                    }
                    else
                    {
                        nbrUpperDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
                    }
                }
                nbrCell.localDir = math.normalizesafe(nbrLowerDir) + Constants.c_avoid * math.exp(-pgaInms2) * math.normalizesafe(nbrUpperDir);
                cells[nbrFlatIndex] = nbrCell;
                n2nList.Dispose();

                // 计算当前网格的局部指导方向
                diff = curLocalCost - nbrLocalCost;
                if (diff == 0) continue;
                dir = nbrCell.gridIndex - curCell.gridIndex;
                if (diff > 0)
                {
                    curLowerDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
                }
                else
                {
                    curUpperDir += (diff / (math.abs(dir.x) + math.abs(dir.y))) * dir;
                }
            }
            curCell.localDir = math.normalizesafe(curLowerDir) + Constants.c_avoid * math.exp(-pgaInms2) * math.normalizesafe(curUpperDir);
            cells[curIndex] = curCell;
            nbrList.Dispose();
        }
    }
}