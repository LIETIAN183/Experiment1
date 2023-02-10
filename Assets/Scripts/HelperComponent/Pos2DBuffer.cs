using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(250)]
public struct Pos2DBuffer : IBufferElementData
{
    public float2 pos2D;

    public static implicit operator float2(Pos2DBuffer pos2DBufferElement) => pos2DBufferElement.pos2D;

    public static implicit operator Pos2DBuffer(float2 p) => new Pos2DBuffer { pos2D = p };
}
