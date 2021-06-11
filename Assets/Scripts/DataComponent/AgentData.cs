using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AgentData : IComponentData
{
    public int fieldOfView;//220°

    public float3 targetDirection;

    public float3 escapeDirection;//为 float3.zero时表示不清楚出口位置

    public float3 fromDirection;

    public quaternion targetRotation;
}
