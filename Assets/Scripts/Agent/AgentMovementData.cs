using Unity.Entities;
using Unity.Mathematics;

public enum AgentState { NotActive, Delay, Escape, Escaped };
[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float stdVel;

    public AgentState state;

    public float reactionTimeVariable;

    // 用于分析
    public float3 originPosition;
    public float reactionTime;

    public float escapeTime;

    public float pathLength;

    public float3 lastPosition;

    public float curVel;

    public float stepDuration;
}