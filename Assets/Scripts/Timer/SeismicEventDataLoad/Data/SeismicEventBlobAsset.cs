using Unity.Mathematics;
using Unity.Entities;

public struct SeismicEventBlobAsset
{
    public BlobArray<float3> eventAccArray;
    public BlobString eventName;
    public float eventDeltaTime;
}