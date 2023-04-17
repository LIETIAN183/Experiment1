using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AgentMovementDataAuthoring : MonoBehaviour
{
    public float standardVel;
    class Baker : Baker<AgentMovementDataAuthoring>
    {
        public override void Bake(AgentMovementDataAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new AgentMovementData
            {
                stdVel = authoring.standardVel,
                deltaHeight = 0,
                forceForFootInteraction = 0,
                desireSpeed = 0,
                curSpeed = 0
            });
        }
    }
}


public struct AgentMovementData : IComponentData
{
    public float stdVel;
    public float3 originPosition;

    public float3 forceForFootInteraction;

    public float desireSpeed;

    public float curSpeed;
    public float deltaHeight;

    public float familiarity;
    public float reactionCofficient;

    public bool SeeExit;

    public float2 lastSelfDir;

    public float angle;
}