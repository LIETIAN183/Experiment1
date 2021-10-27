using Unity.Entities;
using Unity.Mathematics;

public enum AgentState { NotActive, Delay, Escape };
[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float desireSpeed;

    // public float2 desireDirection;

    public AgentState state;

    public float reactionTimeVariable;

    // 用于分析
    public float reactionTime;

    public float k1, k2, k3;
}