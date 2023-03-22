using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;

// TODO: 单次仿真后，行人不销毁，因此下一次单次仿真出错
// 不管单次还是多次仿真结束后，都应该销毁 Agent
/// <summary>
/// Spawner Agent Job
/// </summary>
[BurstCompile]
[WithAll(typeof(SpawnerData))]
partial struct SpawnerAgentJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public uint randomInitSeed;

    void Execute(ref SpawnerData spawner, ref DynamicBuffer<PosBuffer> buffer)
    {
        var posBuffer = buffer.Reinterpret<float3>();
        ComponentTypeSet components = new ComponentTypeSet(ComponentType.ReadWrite<Idle>(), ComponentType.ReadWrite<Escaping>(), ComponentType.ReadWrite<Escaped>());
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        var random = Random.CreateFromIndex(0);
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
                // position = new float3(-0.4f, 1f, -3.75f);
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
            ecb.SetComponent<LocalTransform>(spawnedEntity, LocalTransform.FromPosition(position));
            posBuffer.Add(position);
            spawner.currentCount++;
        }
        outHits.Dispose();
    }
}