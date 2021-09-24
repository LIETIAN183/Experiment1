using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AimTag : IComponentData
{
    public float3 lastPosition;
    public float3 delta;
}