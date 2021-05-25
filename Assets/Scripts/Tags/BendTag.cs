using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BendTag : IComponentData
{
    public quaternion baseRotation;
}