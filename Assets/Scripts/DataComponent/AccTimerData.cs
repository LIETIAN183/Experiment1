using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
[GenerateAuthoringComponent]
public struct AccTimerData : IComponentData
{
    public int gmIndex;

    public float dataDeltaTime;
    public int timeCount;
    public int increaseNumber;
    public float3 acc;

    public float elapsedTime;
}
