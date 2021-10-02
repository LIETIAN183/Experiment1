using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SubShakeData : IComponentData
{
    public float3 originLocalPosition;

    public float height;
}
