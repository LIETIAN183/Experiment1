using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

[GenerateAuthoringComponent]
public struct BreakableTag : IComponentData
{
    public Entity breakedPrefabs;

    public float3 targetPosition;

    public int index;
}
