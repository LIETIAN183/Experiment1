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