using Unity.Mathematics;
using Unity.Entities;

public struct GroundMotion
{
    public float3 acceleration;
}

public struct GroundMotionBlobAsset
{
    public BlobArray<GroundMotion> gmArray;
    public BlobString gmName;
    public float deltaTime;
}