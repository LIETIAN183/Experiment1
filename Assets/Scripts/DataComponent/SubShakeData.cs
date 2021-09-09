using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[GenerateAuthoringComponent]
public struct SubShakeData : IComponentData
{
    public float3 originLocalPosition;

    public float height;

    // 添加刚体后解除物体的父子关系，所以需要手动添加引用
    public Entity parent;
}
