using Unity.Entities;
using Unity.Mathematics;

public struct OriginRelativePos_RotInfo : IComponentData
{
    public float3 orgPos;
    public quaternion orgRot;
}