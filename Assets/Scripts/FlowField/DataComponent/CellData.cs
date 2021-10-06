using Unity.Entities;
using Unity.Mathematics;

public struct CellData
{
    public float3 worldPos;
    public int2 gridIndex;
    public byte cost;
    public ushort bestCost;
    public int2 bestDirection;
}
