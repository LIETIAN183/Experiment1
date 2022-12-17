using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public struct SpawnerData : IComponentData
{
    public bool canSpawn;
    public int desireCount;
    public int currentCount;
    public float3 center;
    public float sideLength;
    public Entity Prefab;
}

public class SpawnerDataAuthoring : MonoBehaviour { }
public class SpawnerDataAuthoringBaker : Baker<SpawnerDataAuthoring>
{
    public override void Bake(SpawnerDataAuthoring authoring)
    {
        AddComponent<SpawnerData>();
    }
}