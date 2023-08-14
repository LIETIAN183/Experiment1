using Unity.Mathematics;
using Unity.Entities;

public struct SeismicEventBlobAsset
{
    // 加速度序列数组
    public BlobArray<float3> eventAccArray;
    // 地震事件名称
    public BlobString eventName;
    // 地震事件记录数据之间的间隔时间
    public float eventDeltaTime;
}