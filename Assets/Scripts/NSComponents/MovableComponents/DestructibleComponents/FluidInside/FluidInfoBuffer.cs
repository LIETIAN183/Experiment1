using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


[InternalBufferCapacity(100)]
public struct FluidInfoBuffer : IBufferElementData
{
    public fluidInfo info;

    public static implicit operator fluidInfo(FluidInfoBuffer fluidInfoBuffer) => fluidInfoBuffer.info;

    public static implicit operator FluidInfoBuffer(fluidInfo i) => new FluidInfoBuffer { info = i };
}

public struct fluidInfo
{
    public float3 position;
    public quaternion rotation;
}