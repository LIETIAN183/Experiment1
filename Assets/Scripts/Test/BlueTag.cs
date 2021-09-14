using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BlueTag : IComponentData
{
    public float3 acc;
}