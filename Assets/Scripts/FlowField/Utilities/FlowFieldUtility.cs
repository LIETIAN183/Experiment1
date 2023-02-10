using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

public static class FlowFieldUtility
{
    /// <summary>
    /// 8邻域
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="gridSize"></param>
    /// <returns></returns>
    public static NativeList<int2> Get8NeighborIndices(int2 currentIndex, int2 gridSize)//IEnumerable<GridDirection> directions
    {
        var neighbors = new NativeList<int2>(Allocator.Temp);
        foreach (int2 relativeDir in GridDirection.EightDirections)
        {
            int2 neighborIndex = GetIndexAtRelativePosition(currentIndex, relativeDir, gridSize);
            if (neighborIndex.x >= 0) neighbors.Add(neighborIndex);
        }
        return neighbors;
    }

    public static NativeList<int> Get8NeighborFlatIndices(int2 currentIndex, int2 gridSize)
    {
        var neighbors = new NativeList<int>(Allocator.Temp);
        foreach (int2 relativeDir in GridDirection.EightDirections)
        {
            int neighborFlatIndex = GetFlatIndexAtRelativePosition(currentIndex, relativeDir, gridSize);
            if (neighborFlatIndex >= 0) neighbors.Add(neighborFlatIndex);
        }
        return neighbors;
    }

    public static NativeList<int> Get8NeighborFlatIndicesIncluedEdgeInOrder(int2 currentIndex, int2 gridSize)
    {
        var neighbors = new NativeList<int>(Allocator.Temp);
        foreach (int2 relativeDir in GridDirection.EightDirections)
        {
            neighbors.Add(GetFlatIndexAtRelativePosition(currentIndex, relativeDir, gridSize));
        }
        return neighbors;
    }
    /// <summary>
    /// 4邻域
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="gridSize"></param>
    /// <returns></returns>
    public static NativeList<int2> Get4NeighborIndices(int2 currentIndex, int2 gridSize)
    {
        var neighbors = new NativeList<int2>(Allocator.Temp);
        foreach (int2 relativeDir in GridDirection.FourDirections)
        {
            int2 neighborIndex = GetIndexAtRelativePosition(currentIndex, relativeDir, gridSize);
            if (neighborIndex.x >= 0) neighbors.Add(neighborIndex);
        }
        return neighbors;
    }
    public static int2 GetIndexAtRelativePosition(int2 currentPos, int2 relativePos, int2 gridSize)
    {
        return DeterminingInGridSet(currentPos + relativePos, gridSize);
    }

    public static int GetFlatIndexAtRelativePosition(int2 currentPos, int2 relativePos, int2 gridSize)
    {
        return DeterminingInGridSet(currentPos + relativePos, gridSize).Equals(Constants.notInGridSet) ? -1 : ToFlatIndex(currentPos + relativePos, gridSize.y);
    }
    public static int ToFlatIndex(int2 index2D, int columnNumber) => columnNumber * index2D.x + index2D.y;
    public static int ToFlatIndex(int x, int y, int columnNumber) => columnNumber * x + y;
    public static int2 GetCellIndexFromWorldPos(float3 worldPos, float3 originPoint, int2 gridSize, float3 cellDiameter)
    {
        float percentX = (worldPos.x - originPoint.x) / (gridSize.x * cellDiameter.x);
        float percentY = (worldPos.z - originPoint.z) / (gridSize.y * cellDiameter.z);

        percentX = math.clamp(percentX, 0f, 1f);
        percentY = math.clamp(percentY, 0f, 1f);

        int2 cellIndex = new int2
        {
            x = math.clamp((int)math.floor((gridSize.x) * percentX), 0, gridSize.x - 1),
            y = math.clamp((int)math.floor((gridSize.y) * percentY), 0, gridSize.y - 1)
        };
        return DeterminingInGridSet(cellIndex, gridSize);
    }

    /// <summary>
    /// 返回在数组中的索引，若返回-1表示该点不位于网格集合中
    /// </summary>
    /// <param name="worldPos"></param>
    /// <param name="originPoint"></param>
    /// <param name="gridSize"></param>
    /// <param name="cellDiameter"></param>
    /// <returns></returns>
    public static int GetCellFlatIndexFromWorldPos(float2 worldPos, float3 originPoint, int2 gridSize, float3 cellDiameter)
    {
        float percentX = (worldPos.x - originPoint.x) / (gridSize.x * cellDiameter.x);
        float percentY = (worldPos.y - originPoint.z) / (gridSize.y * cellDiameter.z);

        percentX = math.clamp(percentX, 0f, 1f);
        percentY = math.clamp(percentY, 0f, 1f);

        int2 cellIndex = new int2
        {
            x = math.clamp((int)math.floor((gridSize.x) * percentX), 0, gridSize.x - 1),
            y = math.clamp((int)math.floor((gridSize.y) * percentY), 0, gridSize.y - 1)
        };
        return DeterminingInGridSet(cellIndex, gridSize).Equals(Constants.notInGridSet) ? -1 : ToFlatIndex(cellIndex, gridSize.y);
    }

    public static int2 DeterminingInGridSet(int2 index, int2 gridSize)
    {
        return (index.x < 0 || index.y < 0 || index.x >= gridSize.x || index.y >= gridSize.y) ? Constants.notInGridSet : index;
    }
}
