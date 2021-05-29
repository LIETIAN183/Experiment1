using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AgentData : IComponentData
{
    public quaternion initRotation;
    public float3 targetDirection;
}
