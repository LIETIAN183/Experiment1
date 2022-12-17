using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct CellData
{
    public float3 worldPos;
    public int2 gridIndex;
    public byte cost;
    public ushort bestCost;
    public float tempCost;
    public int2 bestDirection;
    public float2 bestDir;

    // 用于判断不可通行区域的最外层网格是否更新过
    public bool updated;

    public float sumMass, maxY;
}

[InternalBufferCapacity(250)]
public struct CellBuffer : IBufferElementData
{
    public CellData cell;

    public static implicit operator CellData(CellBuffer cellBufferElement) => cellBufferElement.cell;

    public static implicit operator CellBuffer(CellData e) => new CellBuffer { cell = e };
}

public class CellBufferAuthoring : MonoBehaviour { }
// public class CellBufferElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
// {
//     public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
//     {
//         dstManager.AddBuffer<CellBufferElement>(entity);
//     }
// }

public class CellBufferElementAuthoringBaker : Baker<CellBufferAuthoring>
{
    public override void Bake(CellBufferAuthoring authoring)
    {
        AddBuffer<CellBuffer>();
    }
}

