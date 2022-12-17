using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

public struct BreakableData : IComponentData
{
    public bool fluidInside;
}

public class BreakableDataAuthoring : MonoBehaviour
{
    public bool fluidInside;
}

public class BreakableDataAuthoringBaker : Baker<BreakableDataAuthoring>
{
    public override void Bake(BreakableDataAuthoring authoring)
    {
        AddComponent(new BreakableData { fluidInside = authoring.fluidInside });
    }
}