using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AccData : IComponentData
{
    public float3 applyAcc;
    public float3 currentAcc;

    public float3 lastVelocity;
}
