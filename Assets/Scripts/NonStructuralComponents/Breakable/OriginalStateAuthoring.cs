using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct OriginalState : IComponentData
{
    public float3 originPosition;
    public quaternion originRotation;

    public float3 inverseInertia;
}

public class OriginalStateAuthoring : MonoBehaviour { }

public class OriginalStateAuthoringBaker : Baker<OriginalStateAuthoring>
{
    public override void Bake(OriginalStateAuthoring authoring)
    {
        AddComponent<OriginalState>();
    }
}
