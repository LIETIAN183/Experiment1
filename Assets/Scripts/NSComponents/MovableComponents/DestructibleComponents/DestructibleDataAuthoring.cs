using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MCDataAuthoring))]
public class DestructibleDataAuthoring : MonoBehaviour
{
    public bool fluidInside;

    public List<GameObject> replaceItems;

    class Baker : Baker<DestructibleDataAuthoring>
    {
        public override void Bake(DestructibleDataAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            AddComponent(entity, new DCData { fluidInside = authoring.fluidInside });

            var entityBuffer = AddBuffer<ReplacePrefabsBuffer>(entity);
            foreach (var item in authoring.replaceItems)
            {
                entityBuffer.Add(new ReplacePrefabsBuffer { replacementItem = GetEntity(item, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace) });
            }
        }
    }
}

public struct DCData : IComponentData
{
    public bool fluidInside;
}
