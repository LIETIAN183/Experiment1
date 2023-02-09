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
            AddComponent(new DCData { fluidInside = authoring.fluidInside });

            var entityBuffer = AddBuffer<ReplacePrefabsBuffer>();
            foreach (var item in authoring.replaceItems)
            {
                entityBuffer.Add(new ReplacePrefabsBuffer { replacementItem = GetEntity(item) });
            }
        }
    }
}

public struct DCData : IComponentData
{
    public bool fluidInside;
}
