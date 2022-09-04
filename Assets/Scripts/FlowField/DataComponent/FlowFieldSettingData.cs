using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FlowFieldSettingData : IComponentData
{
    public float3 originPoint;
    public int2 gridSize;
    public float3 cellRadius;
    public float3 destination;

    public float3 displayHeightOffset;
}

