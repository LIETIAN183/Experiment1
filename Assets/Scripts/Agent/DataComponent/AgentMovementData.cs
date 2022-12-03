using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AgentMovementData : IComponentData
{
    public float stdVel;
    public float3 originPosition;

    public float desireSpeed;

    public float curSpeed;

    public float nextSpeed;
}

