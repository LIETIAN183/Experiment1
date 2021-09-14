using Unity.Entities;
using Unity.Mathematics;
[GenerateAuthoringComponent]
public struct GroundTag : IComponentData
{
    public float3 velocity;
}