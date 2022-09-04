using Unity.Mathematics;

public struct CellData
{
    public float3 worldPos;
    public int2 gridIndex;
    public byte cost;
    public ushort bestCost;
    public int2 bestDirection;

    // 用于判断不可通行区域的最外层网格是否更新过
    public bool updated;

    public float sumMass, maxY;
}
