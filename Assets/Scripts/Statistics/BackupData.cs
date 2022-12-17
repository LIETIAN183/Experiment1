using Unity.Entities;
using Unity.Mathematics;

public struct BackupData : IComponentData
{
    public float3 originPosition;
    public quaternion originRotation;
}
