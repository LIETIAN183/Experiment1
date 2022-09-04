using System.Collections.Generic;
using Unity.Mathematics;

public static class FlowFieldUtility
{
    public static List<int2> GetNeighborIndices(int2 currentIndex, int2 gridSize)//IEnumerable<GridDirection> directions
    {
        var neighbors = new List<int2>();
        foreach (int2 relativeDir in GridDirection.CardinalAndIntercardinalDirections)
        {
            int2 neighborIndex = GetIndexAtRelativePosition(currentIndex, relativeDir, gridSize);
            if (neighborIndex.x >= 0) neighbors.Add(neighborIndex);
        }
        return neighbors;
    }
    public static int2 GetIndexAtRelativePosition(int2 currentPos, int2 relativePos, int2 gridSize)
    {
        int2 finalPos = currentPos + relativePos;
        return (finalPos.x < 0 || finalPos.x >= gridSize.x || finalPos.y < 0 || finalPos.y >= gridSize.y) ? new int2(-1, -1) : finalPos;
    }
    public static int ToFlatIndex(int2 index2D, int height) => height * index2D.x + index2D.y;
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
        return cellIndex;
    }
}
