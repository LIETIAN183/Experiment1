using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Physics;
// using System.Collections.Generic;
// using UnityEngine;
// using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    private BuildPhysicsWorld buildPhysicsWorld;

    public float spawnerDelayTime = 2f;
    public float timer;
    public static readonly float3 halfExtents = new float3(0.25f, 0.85f, 0.25f);


    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }


    protected override void OnUpdate()
    {
        // 延迟初始化
        timer += Time.DeltaTime;
        if (timer < spawnerDelayTime) return;

        // Random random = new Random(30);
        Random random = new Random(30);
        // random.InitState();

        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer();
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);

        // Debug.Log(outHits.Length);
        Entities.WithReadOnly(physicsWorld).ForEach((ref SpawnerData spawner, ref DynamicBuffer<PosBufferElement> buffer) =>
        {
            while (spawner.currentCount < spawner.desireCount)
            {

                spawner.currentCount++;
                Entity spawnedEntity = commandBuffer.Instantiate(spawner.Prefab);
                float3 position;

                bool flag;
                DynamicBuffer<float3> posBuffer = buffer.Reinterpret<float3>();
                // 由于Agent生成延迟，因此碰撞检测时只能检测到商品等物体，无法检测到其他Agent
                do
                {
                    outHits.Clear();
                    flag = true;
                    position = spawner.center + new float3(random.NextFloat(-spawner.sideLength, spawner.sideLength), 0, random.NextFloat(-spawner.sideLength, spawner.sideLength));
                    physicsWorld.OverlapBox(position, quaternion.identity, halfExtents, ref outHits, CollisionFilter.Default);

                    foreach (var pos in posBuffer)
                    {
                        if (math.distance(pos, position) < 0.6f)
                        {
                            flag = false;
                            break;
                        }
                    }
                } while (outHits.Length != 0 || !flag);
                commandBuffer.SetComponent<Translation>(spawnedEntity, new Translation { Value = position });
                posBuffer.Add(position);

                var mass = GetComponent<PhysicsMass>(spawner.Prefab);
                mass.InverseInertia = float3.zero;
                commandBuffer.SetComponent<PhysicsMass>(spawnedEntity, mass);
            }
        }).Run();
        outHits.Dispose();
    }
}
