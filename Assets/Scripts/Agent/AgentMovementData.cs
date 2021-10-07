using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float moveSpeed;

    public float2 direction;

    public int2 index;
    public float destinationMoveSpeed;
    public bool destinationReached;
}