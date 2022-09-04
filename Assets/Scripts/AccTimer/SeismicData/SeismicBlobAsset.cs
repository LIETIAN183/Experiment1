using Unity.Mathematics;
using Unity.Entities;

public struct SeismicBlobAsset
{
    public BlobArray<float3> seismicAccArray;
    public BlobString seismicName;
    public float dataDeltaTime;
}