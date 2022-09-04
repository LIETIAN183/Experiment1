using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SubFCData : IComponentData
{
    public float3 originLocalPosition;

    public quaternion originalRotation;

    public float height;

    public Entity parent;
}
