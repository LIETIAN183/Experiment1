using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FlowFieldSettingData : IComponentData
{
    public float3 originPoint;
    public int2 gridSize;
    public float cellRadius;
    public float3 destination;
    public int2 destinationIndex;
}

