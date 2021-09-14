using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SyncTag : IComponentData
{
    public float3 acc;
}