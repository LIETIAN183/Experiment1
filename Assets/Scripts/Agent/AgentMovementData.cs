using Unity.Entities;
using Unity.Mathematics;

public enum AgentState { NotActive, Delay, Escape };
[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float desireSpeed;

    public float2 desireDirection;

    public AgentState state;

    public float delayTimeVariable;
}