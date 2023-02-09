using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnerDataAuthoring : MonoBehaviour
{
    public GameObject agentPrefab;
    public class SpawnerDataAuthoringBaker : Baker<SpawnerDataAuthoring>
    {
        public override void Bake(SpawnerDataAuthoring authoring)
        {
            AddComponent(new SpawnerData
            {
                prefab = GetEntity(authoring.agentPrefab),
                center = new float3(0, 0.9f, 0),
                sideLength = 10
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