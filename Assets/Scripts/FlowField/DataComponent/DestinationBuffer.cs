using Unity.Entities;

[InternalBufferCapacity(100)]
public struct DestinationBuffer : IBufferElementData
{
    public int desFlatIndex;

    public static implicit operator int(DestinationBuffer destinationBufferElement) => destinationBufferElement.desFlatIndex;

    public static implicit operator DestinationBuffer(int index) => new DestinationBuffer { desFlatIndex = index };
}