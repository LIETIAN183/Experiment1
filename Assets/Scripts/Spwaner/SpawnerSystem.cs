using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup))]
// [DisableAutoCreation]
public class SpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    private BuildPhysicsWorld buildPhysicsWorld;

    public float spawnerDelayTime = 2f;
    public float timer;
    private static readonly float3 halfExtents = new float3(0.25f, 0.25f, 0.25f);
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }


    protected override void OnUpdate()
    {
        Random random = new Random();
        random.InitState();
        // 延迟初始化
        timer += Time.DeltaTime;
        if (timer < spawnerDelayTime) return;

        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer();
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);
        // Debug.Log(outHits.Length);
        Entities.WithReadOnly(physicsWorld).ForEach((ref SpawnerData spawner) =>
        {
            while (spawner.currentCount++ < spawner.desireCount)
            {
                Entity spawnedEntity = commandBuffer.Instantiate(spawner.Prefab);
                float3 position;
                do
                {
                    outHits.Clear();
                    position = spawner.center + new float3(random.NextFloat(-spawner.radius, spawner.radius), 0, random.NextFloat(-spawner.radius, spawner.radius));
                    physicsWorld.OverlapBox(position, quaternion.identity, halfExtents, ref outHits, CollisionFilter.Default);
                } while (outHits.Length != 0);
                commandBuffer.SetComponent<Translation>(spawnedEntity, new Translation { Value = position });
            }
        }).Run();
        outHits.Dispose();
    }
}
