using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

// 实现 DCData 数据在编辑器界面的挂载功能，否则仅能通过代码挂载该数据
[RequireComponent(typeof(MCDataAuthoring))]
public class DestructibleDataAuthoring : MonoBehaviour
{
    public bool fluidInside;

    public List<GameObject> replaceItems;

    class Baker : Baker<DestructibleDataAuthoring>
    {
        public override void Bake(DestructibleDataAuthoring authoring)
        {
            // 向目标 Entity 添加 DCData
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            AddComponent(entity, new DCData { fluidInside = authoring.fluidInside });
            // 向目标 Entity 添加破碎模型列表
            var entityBuffer = AddBuffer<ReplacePrefabsBuffer>(entity);
            foreach (var item in authoring.replaceItems)
            {
                entityBuffer.Add(new ReplacePrefabsBuffer { replacementItem = GetEntity(item, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace) });
            }
        }
    }
}

// 破碎材质标签
public struct DCData : IComponentData
{
    // 初始化设置内部是否有液体
    public bool fluidInside;
}
