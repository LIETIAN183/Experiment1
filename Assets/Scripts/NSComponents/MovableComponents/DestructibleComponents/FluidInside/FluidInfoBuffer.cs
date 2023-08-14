using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


[InternalBufferCapacity(100)]
public struct FluidInfoBuffer : IBufferElementData
{
    // 待生成流体的信息
    public fluidInfo info;

    public static implicit operator fluidInfo(FluidInfoBuffer fluidInfoBuffer) => fluidInfoBuffer.info;

    public static implicit operator FluidInfoBuffer(fluidInfo i) => new FluidInfoBuffer { info = i };
}

public struct fluidInfo
{
    // 待生成流体的位置
    public float3 position;
    // 待生成流体的旋转角度
    public quaternion rotation;
}