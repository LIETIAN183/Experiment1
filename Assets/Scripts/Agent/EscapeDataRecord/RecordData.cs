using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RecordData : IComponentData
{
    public float3 lastposition;
    public float escapeTime;
    public float escapeLength;
    public float escapeAveVel;
    public float accumulatedY;
}
