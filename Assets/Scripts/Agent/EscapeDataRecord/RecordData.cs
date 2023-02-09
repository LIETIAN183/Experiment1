using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RecordData : IComponentData
{
    public float3 lastPosition;
    public float escapedTime;
    public float escapedLength;
    public float escapeAveVel;
    public float accumulatedY;
}
