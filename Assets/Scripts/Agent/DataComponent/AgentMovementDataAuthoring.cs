using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct AgentMovementData : IComponentData
{
    public float stdVel;
    public float3 originPosition;

    public float desireSpeed;

    public float curSpeed;

    public float nextSpeed;
}

public class AgentMovementDataAuthoring : MonoBehaviour
{
    public float standardVel;
}

public class AgentMovementDataAuthoringBaker : Baker<AgentMovementDataAuthoring>
{
    public override void Bake(AgentMovementDataAuthoring authoring)
    {
        AddComponent(new AgentMovementData { stdVel = authoring.standardVel });
    }
}
