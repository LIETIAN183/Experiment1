using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AgentSpawnerDataAuthoring : MonoBehaviour
{
    public GameObject agentPrefab;
    class Baker : Baker<AgentSpawnerDataAuthoring>
    {
        public override void Bake(AgentSpawnerDataAuthoring authoring)
        {
            AddComponent(new SpawnerData
            {
                prefab = GetEntity(authoring.agentPrefab),
                center = new float3(0, 0.9f, 0),
                sideLength = 10,
                currentCount = 0,
                desireCount = 0
            });
            AddBuffer<PosBuffer>();
        }
    }
}

public struct SpawnerData : IComponentData
{
    public int desireCount;
    public int currentCount;
    public float3 center;
    public float sideLength;
    public Entity prefab;
}