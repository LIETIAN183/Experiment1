using Unity.Entities;

// 设置初始容量
[InternalBufferCapacity(20)]
public struct BlobRefBuffer : IBufferElementData
{
    // 静态加速度时间序列数据的引用
    public BlobAssetReference<SeismicEventBlobAsset> Value;

    // 数据拆箱
    public static implicit operator BlobAssetReference<SeismicEventBlobAsset>(BlobRefBuffer trajectoryBufferElement) => trajectoryBufferElement.Value;

    // 数据装箱
    public static implicit operator BlobRefBuffer(BlobAssetReference<SeismicEventBlobAsset> e) => new BlobRefBuffer { Value = e };
}