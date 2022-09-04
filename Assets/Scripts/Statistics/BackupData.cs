using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BackupData : IComponentData
{
    public float3 originPosition;
    public quaternion originRotation;
}
