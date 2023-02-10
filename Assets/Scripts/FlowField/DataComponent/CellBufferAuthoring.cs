using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class CellBufferAuthoring : MonoBehaviour
{
    class Baker : Baker<CellBufferAuthoring>
    {
        public override void Bake(CellBufferAuthoring authoring)
        {
            AddBuffer<CellBuffer>();
        }
    }
}

public struct CellData
{
    public float3 worldPos;
    public int2 gridIndex;
    public byte cost;
    public ushort bestCost;
    public float tempCost;
    public float targetCost;
    // public int2 bestDirection;
    public float2 bestDir;
    public float2 targetDir;

    public float3 debugField;

    public static readonly CellData zero = new CellData();

    // 用于判断不可通行区域的最外层网格是否更新过
    // public bool updated;
    // public float sumMass, maxY;
}

[InternalBufferCapacity(250)]
public struct CellBuffer : IBufferElementData
{
    public CellData cell;

    public static implicit operator CellData(CellBuffer cellBufferElement) => cellBufferElement.cell;

    public static implicit operator CellBuffer(CellData e) => new CellBuffer { cell = e };
}