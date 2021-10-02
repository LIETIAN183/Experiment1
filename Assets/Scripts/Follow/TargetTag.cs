using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TargetTag : IComponentData
{
    public float3 previousPosition;

    public float3 deltaMove;
}