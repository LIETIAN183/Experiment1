using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AgentData : IComponentData
{
    public float3 targetDirection;
    public quaternion targetRotation;
}
