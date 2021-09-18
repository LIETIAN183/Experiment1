using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
[GenerateAuthoringComponent]
public struct AccTimerData : IComponentData
{
    public int gmIndex;
    public int timeCount;
    public float3 acc;

    public float elapsedTime;

    public float accMagnitude;
}
