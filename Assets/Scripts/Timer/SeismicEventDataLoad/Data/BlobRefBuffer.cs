using Unity.Entities;

[InternalBufferCapacity(20)]
public struct BlobRefBuffer : IBufferElementData
{
    public BlobAssetReference<SeismicEventBlobAsset> Value;

    public static implicit operator BlobAssetReference<SeismicEventBlobAsset>(BlobRefBuffer trajectoryBufferElement) => trajectoryBufferElement.Value;

    public static implicit operator BlobRefBuffer(BlobAssetReference<SeismicEventBlobAsset> e) => new BlobRefBuffer { Value = e };
}