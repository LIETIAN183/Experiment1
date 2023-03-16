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
    public float localCost;
    public float integrationCost;
    public float2 globalDir;
    public float2 localDir;

    // 用于计算 localCost 的辅助变量
    public float massVariable;
    public float maxHeight;
    public int fluidElementCount;
}

public struct DebugCellData
{
    public int flatIndex;
    public float2 targetDir;
    public float debugField;
}

[InternalBufferCapacity(250)]
public struct CellBuffer : IBufferElementData
{
    public CellData cell;

    public static implicit operator CellData(CellBuffer cellBufferElement) => cellBufferElement.cell;

    public static implicit operator CellBuffer(CellData e) => new CellBuffer { cell = e };
}