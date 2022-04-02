using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent, InternalBufferCapacity(250)]
public struct PosBufferElement : IBufferElementData
{
    public float3 pos;

    public static implicit operator float3(PosBufferElement posBufferElement) => posBufferElement.pos;

    public static implicit operator PosBufferElement(float3 p) => new PosBufferElement { pos = p };
}
