using Unity.Entities;

// 存储网格目标坐标点的数组，支持多个网格目标
[InternalBufferCapacity(100)]
public struct DestinationBuffer : IBufferElementData
{
    // 单个网格目标对应的网格集合Index
    public int desFlatIndex;

    public static implicit operator int(DestinationBuffer destinationBufferElement) => destinationBufferElement.desFlatIndex;

    public static implicit operator DestinationBuffer(int index) => new DestinationBuffer { desFlatIndex = index };
}