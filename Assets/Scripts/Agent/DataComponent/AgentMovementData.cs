using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float stdVel;
    public float3 originPosition;
}

