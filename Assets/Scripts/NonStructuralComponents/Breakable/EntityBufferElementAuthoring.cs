using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

// 虽然每个每个要破碎的物体都独立挂载一个prefab列表，但是列表内的prefab都是引用，因此初始化的时候预制体初始化的个数是恒定的，不随着破碎物体数量的增长而增长
[InternalBufferCapacity(10)]
public struct EntityBufferElement : IBufferElementData
{
    public Entity replacementItem;

    public static implicit operator Entity(EntityBufferElement entityBufferElement) => entityBufferElement.replacementItem;

    public static implicit operator EntityBufferElement(Entity e) => new EntityBufferElement { replacementItem = e };
}

public class EntityBufferElementAuthoring : MonoBehaviour
{
    public List<GameObject> replaceItems;
}

public class EntityBufferElementAuthoringBaker : Baker<EntityBufferElementAuthoring>
{
    public override void Bake(EntityBufferElementAuthoring authoring)
    {
        var entityBuffer = AddBuffer<EntityBufferElement>();
        foreach (var item in authoring.replaceItems)
        {
            entityBuffer.Add(new EntityBufferElement { replacementItem = GetEntity(item) });
        }
    }
}

