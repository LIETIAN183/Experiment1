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
partial struct SpawnerAgentJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public uint randomInitSeed;
    [ReadOnly] public ComponentLookup<PhysicsMass> massList;

    void Execute(ref SpawnerData spawner, ref DynamicBuffer<PosBuffer> buffer)
    {
        var posBuffer = buffer.Reinterpret<float3>();
        ComponentTypeSet components = new ComponentTypeSet(ComponentType.ReadWrite<Idle>(), ComponentType.ReadWrite<Escaping>(), ComponentType.ReadWrite<Escaped>());
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
        var random = Random.CreateFromIndex(randomInitSeed);
        var random2 = Random.CreateFromIndex(10);
        while (spawner.currentCount < spawner.desireCount)
        {
            var spawnedEntity = ecb.Instantiate(spawner.prefab);
            float3 position = float3.zero;
            bool flag;

            do
            {
                outHits.Clear();
                flag = true;
                // position = spawner.center + new float3(random.NextFloat(-spawner.sideLength, spawner.sideLength), 0, random.NextFloat(-spawner.sideLength, spawner.sideLength));
                position = spawner.center + new float3(random.NextFloat(-5, 5), 0, random.NextFloat(-10, 7.5f));
                // position = spawner.center + new float3(random.NextFloat(6, 10), 0, random.NextFloat(-10, 7.5f));
                physicsWorld.OverlapBox(position, quaternion.identity, Constants.halfHumanSize3D, ref outHits, Constants.agentWallOnlyFilter);

                foreach (var pos in posBuffer)
                {
                    if (math.distance(pos, position) < 0.6f)
                    {
                        flag = false;
                        break;
                    }
                }
            } while (outHits.Length != 0 || !flag);

            ecb.SetComponent<AgentMovementData>(spawnedEntity, new AgentMovementData
            {
                forceForFootInteraction = 0,
                desireSpeed = 0,
                deltaHeight = 0,
                familiarity = NormalDistribution.RandomGaussianInRange(0f, 1, random2.NextUInt()),
                reactionCofficient = NormalDistribution.RandomGaussianInRange(0.7f, 1.3f, random2.NextUInt()),
                SeeExit = false,
                fallTimer = 2f
            });
            ecb.AddComponent<RecordData>(spawnedEntity);
            ecb.AddBuffer<PosBuffer>(spawnedEntity);
            ecb.AddComponent(spawnedEntity, components);
            ecb.SetComponentEnabled<Escaping>(spawnedEntity, false);
            ecb.SetComponentEnabled<Escaped>(spawnedEntity, false);
            var dir = random.NextFloat2Direction();
            ecb.SetComponent<LocalTransform>(spawnedEntity, LocalTransform.FromPositionRotation(position, quaternion.LookRotationSafe(new float3(dir.x, 0, dir.y), math.up())));
            posBuffer.Add(position);

            var mass = massList[spawner.prefab];
            mass.InverseInertia = float3.zero;
            ecb.SetComponent<PhysicsMass>(spawnedEntity, mass);
            spawner.currentCount++;
        }
        outHits.Dispose();
    }
}