using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct OriginalState : IComponentData
{
    public float3 originPosition;
    public quaternion originRotation;

    public float3 inverseInertia;
}
