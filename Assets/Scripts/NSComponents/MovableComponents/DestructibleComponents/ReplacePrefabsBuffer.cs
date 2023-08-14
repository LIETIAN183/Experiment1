using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

// 虽然每个每个要破碎的物体都独立挂载一个prefab列表，但是列表内的prefab都是引用，因此初始化的时候预制体初始化的个数是恒定的，不随着破碎物体数量的增长而增长
[InternalBufferCapacity(10)]
public struct ReplacePrefabsBuffer : IBufferElementData
{
    // 可用的替换破碎模型
    public Entity replacementItem;

    public static implicit operator Entity(ReplacePrefabsBuffer entityBufferElement) => entityBufferElement.replacementItem;

    public static implicit operator ReplacePrefabsBuffer(Entity e) => new ReplacePrefabsBuffer { replacementItem = e };
}