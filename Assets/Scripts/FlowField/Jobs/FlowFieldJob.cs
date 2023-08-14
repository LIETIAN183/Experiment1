using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

/// <summary>
/// 使用集成代价，计算非目标网格的全局指导方向
/// </summary>
[BurstCompile]
public struct CalculateGlobalFlowFieldJob_NotDestGrid : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public float pgaInms2;
    [ReadOnly] public int2 gridSetSize;
    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        float2 lowerDir = float2.zero, upperDir = float2.zero, dir;
        float diff;
        var flatNeighborIndexList = FlowFieldUtility.Get8NeighborFlatIndices(curCell.gridIndex, gridSetSize);
        //计算周围八个网格，得到加权梯度乘以相应的方向向量，计算褚网格的全局指导方向
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

        curCell.globalDir = math.normalizesafe(lowerDir) + Constants.w_avoid * math.exp(-pgaInms2) * math.normalizesafe(upperDir);
        cells[flatIndex] = curCell;
    }
}

/// <summary>
/// 设置目标网格的全局指导方向，即无方向
/// </summary>
[BurstCompile]
public struct CalculateGlobalFlowFieldJob_DestGrid : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public NativeArray<int> dests;
    public void Execute(int flatIndex)
    {
        var curCell = cells[dests[flatIndex]];
        curCell.globalDir = float2.zero;
        cells[dests[flatIndex]] = curCell;
    }
}

/// <summary>
/// 使用网格总代价，计算非目标网格的局部指导方向
/// </summary>
[BurstCompile]
public struct CalculateLocalFlowFieldJob_NotDestGrid : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public float pgaInms2;
    [ReadOnly] public int2 gridSetSize;
    public void Execute(int flatIndex)
    {
        var curCell = cells[flatIndex];
        float2 lowerDir = float2.zero, upperDir = float2.zero, dir;
        float diff;
        var flatNeighborIndexList = FlowFieldUtility.Get8NeighborFlatIndices(curCell.gridIndex, gridSetSize);
        foreach (int flatNeighborIndex in flatNeighborIndexList)
        {
            CellData neighborCell = cells[flatNeighborIndex];
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

        // curCell.localDir = math.normalizesafe(lowerDir) + Constants.c_avoid * math.exp(-pgaInms2) * math.normalizesafe(upperDir);
        curCell.localDir = math.normalizesafe(lowerDir) + Constants.w_avoid * math.exp(-pgaInms2) * math.normalizesafe(upperDir);
        cells[flatIndex] = curCell;
    }
}

/// <summary>
/// 计算目标网格的局部指导方向，此时不考虑总代价计算中的行人影响
/// </summary>
[BurstCompile]
public struct CalculateLocalFlowFieldJob_DestGrid : IJob
{
    [NativeDisableParallelForRestriction]
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
                curLocalCost = math.exp(-pgaInms2) * (curCell.massVariable + Constants.c2_fluid * curCell.fluidElementCount * 0.0083f) / gridVolume + (uint)(math.exp(curCell.maxHeight) + curCell.maxHeight * Constants.c_s);
            }
            // 在计算目标网格的网格局部指导方向时，由于目标网格的邻接网格也可能是目标网格，而在代价场计算中得到的网格总代价均考虑了人群密度影响，因此在本计算过程中，当前玩个够以及其所有邻接网格都需要重新计算网格总代价并排除人群密度影响。
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
                    nbrLocalCost = math.exp(-pgaInms2) * (nbrCell.massVariable + Constants.c2_fluid * nbrCell.fluidElementCount * 0.0083f) / gridVolume + (uint)(math.exp(nbrCell.maxHeight) + curCell.maxHeight * Constants.c_s);
                }
                //因邻接网格也可能是目标网格，所以需要计算邻接网格的本地指导方向
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
                        n2nLocalCost = math.exp(-pgaInms2) * (n2nCell.massVariable + Constants.c2_fluid * n2nCell.fluidElementCount * 0.0083f) / gridVolume + (uint)(math.exp(n2nCell.maxHeight) + curCell.maxHeight * Constants.c_s);
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
                nbrCell.localDir = math.normalizesafe(nbrLowerDir) + Constants.w_avoid * math.exp(-pgaInms2) * math.normalizesafe(nbrUpperDir);
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
            curCell.localDir = math.normalizesafe(curLowerDir) + Constants.w_avoid * math.exp(-pgaInms2) * math.normalizesafe(curUpperDir);
            cells[curIndex] = curCell;
            nbrList.Dispose();
        }
    }
}