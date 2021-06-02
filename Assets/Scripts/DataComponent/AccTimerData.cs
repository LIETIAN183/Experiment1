using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AccTimerData : IComponentData
{
    public int gmIndex;
    public int timeCount;
    public float3 acc;
}
