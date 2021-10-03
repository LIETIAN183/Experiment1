using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[GenerateAuthoringComponent]
public struct SubShakeData : IComponentData
{
    public float3 originLocalPosition;

    public float height;
}
