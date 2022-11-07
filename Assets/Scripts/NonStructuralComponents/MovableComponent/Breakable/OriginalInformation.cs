using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct OriginalInformation : IComponentData
{
    public float3 originPosition;
    public quaternion originRotation;
}
