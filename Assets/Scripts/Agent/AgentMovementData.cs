using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float desireSpeed;

    public float2 desireDirection;

}