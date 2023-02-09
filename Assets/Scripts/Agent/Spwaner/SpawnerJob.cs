using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;

/// <summary>
/// Spawner Agent Job
/// </summary>
[BurstCompile]
[WithAll(typeof(SpawnerData))]
partial struct SpawnerJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public uint randomInitSeed;

    void Execute(ref SpawnerData spawner, ref DynamicBuffer<PosBuffer> buffer)
    {
        var posBuffer = buffer.Reinterpret<float3>();
        ComponentTypeSet components = new ComponentTypeSet(ComponentType.ReadWrite<Idle>(), ComponentType.ReadWrite<Escaping>(), ComponentType.ReadWrite<Escaped>());
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        var random = new Random();
        random.InitState(randomInitSeed);
        while (spawner.currentCount < spawner.desireCount)
        {
            var spawnedEntity = ecb.Instantiate(spawner.prefab);
            float3 position;
            bool flag;
            do
            {
                outHits.Clear();
                flag = true;
                position = spawner.center + new float3(random.NextFloat(-spawner.sideLength, spawner.sideLength), 0, random.NextFloat(-spawner.sideLength, spawner.sideLength));
                physicsWorld.OverlapBox(position, quaternion.identity, Constants.halfHumanSize3D, ref outHits, CollisionFilter.Default);

                foreach (var pos in posBuffer)
                {
                    if (math.distance(pos, position) < 0.6f)
                    {
                        flag = false;
                        break;
                    }
                }
            } while (outHits.Length != 0 || !flag);
            ecb.AddComponent<RecordData>(spawnedEntity);
            ecb.AddBuffer<PosBuffer>(spawnedEntity);
            ecb.AddComponent(spawnedEntity, components);
            ecb.SetComponentEnabled<Escaping>(spawnedEntity, false);
            ecb.SetComponentEnabled<Escaped>(spawnedEntity, false);
            // ecb.SetComponent<LocalTransform>(spawnedEntity, LocalTransform.FromPositionRotationScale(new float3(3, 1, -3.5f), quaternion.identity, 0.5f));
            ecb.SetComponent<LocalTransform>(spawnedEntity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 0.5f));
            posBuffer.Add(position);
            spawner.currentCount++;
        }
        outHits.Dispose();
    }
}