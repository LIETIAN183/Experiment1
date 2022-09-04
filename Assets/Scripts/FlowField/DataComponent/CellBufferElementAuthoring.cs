using UnityEngine;
using Unity.Entities;

public class CellBufferElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<CellBufferElement>(entity);
    }
}
