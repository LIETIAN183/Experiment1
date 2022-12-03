using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class EntityBufferElementAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public List<GameObject> replaceItems;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var entityBuffer = dstManager.AddBuffer<EntityBufferElement>(entity);
        foreach (var item in replaceItems)
        {
            entityBuffer.Add(new EntityBufferElement { replacementItem = conversionSystem.GetPrimaryEntity(item) });
        }

    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(replaceItems);
    }
}

// 虽然每个每个要破碎的物体都独立挂载一个prefab列表，但是列表内的prefab都是引用，因此初始化的时候预制体初始化的个数是恒定的，不随着破碎物体数量的增长而增长
[InternalBufferCapacity(10)]
public struct EntityBufferElement : IBufferElementData
{
    public Entity replacementItem;

    public static implicit operator Entity(EntityBufferElement entityBufferElement) => entityBufferElement.replacementItem;

    public static implicit operator EntityBufferElement(Entity e) => new EntityBufferElement { replacementItem = e };
}
