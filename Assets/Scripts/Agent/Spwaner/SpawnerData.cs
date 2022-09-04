using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SpawnerData : IComponentData
{
    public bool canSpawn;
    public int desireCount;
    public int currentCount;
    public float3 center;
    public float sideLength;
    public Entity Prefab;
}
