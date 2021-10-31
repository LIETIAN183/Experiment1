using Unity.Entities;
using Unity.Mathematics;

public enum AgentState { NotActive, Delay, Escape, Escaped };
[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float desireSpeed;

    // public float2 desireDirection;

    public AgentState state;

    public float reactionTimeVariable;

    // 用于分析
    public float reactionTime;

    public float3 originPosition;
}