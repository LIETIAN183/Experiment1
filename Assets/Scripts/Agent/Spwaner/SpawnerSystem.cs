using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Physics;
using System.Threading.Tasks;

[UpdateInGroup(typeof(SimulationSystemGroup))]
// [UpdateBefore(typeof(TransformSystemGroup))]
public partial class SpawnerSystem : SystemBase
{

    public static readonly float3 halfHumanSize3D = new float3(0.25f, 0.85f, 0.25f);

    protected override void OnStartRunning()
    {
        var spawnerData = GetSingleton<SpawnerData>();
        spawnerData.canSpawn = false;
        SetSingleton(spawnerData);
    }

    protected override void OnUpdate()
    {
        var simulationSetting = GetSingleton<SimulationLayerConfigurationData>();
        if (!simulationSetting.isSimulateAgent)
        {
            return;
        }

        Random random = new Random(30);

        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        var commandBuffer = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();
        NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);

        // Debug.Log(outHits.Length);
        Entities.WithReadOnly(physicsWorld).ForEach((ref SpawnerData spawner, ref DynamicBuffer<PosBuffer> buffer) =>
        {
            if (!spawner.canSpawn) return;
            DynamicBuffer<float3> posBuffer = buffer.Reinterpret<float3>();
            // 由于Agent生成延迟，因此碰撞检测时只能检测到商品等物体，无法检测到其他Agent
            while (spawner.currentCount < spawner.desireCount)
            {
                Entity spawnedEntity = commandBuffer.Instantiate(spawner.Prefab);
                float3 position;

                bool flag = true;

                do
                {
                    outHits.Clear();
                    flag = true;
                    position = spawner.center + new float3(random.NextFloat(-spawner.sideLength, spawner.sideLength), 0, random.NextFloat(-spawner.sideLength, spawner.sideLength));
                    physicsWorld.OverlapBox(position, quaternion.identity, halfHumanSize3D, ref outHits, CollisionFilter.Default);

                    foreach (var pos in posBuffer)
                    {
                        if (math.distance(pos, position) < 0.6f)
                        {
                            flag = false;
                            break;
                        }
                    }
                } while (outHits.Length != 0 || !flag);
                commandBuffer.AddComponent<Idle>(spawnedEntity);
                commandBuffer.AddComponent<RecordData>(spawnedEntity);
                commandBuffer.SetComponent<LocalTransform>(spawnedEntity, new LocalTransform { Position = position });
                // commandBuffer.SetComponent<Translation>(spawnedEntity, new Translation { Value = position });
                posBuffer.Add(position);

                var mass = GetComponent<PhysicsMass>(spawner.Prefab);
                mass.InverseInertia = float3.zero;
                commandBuffer.SetComponent<PhysicsMass>(spawnedEntity, mass);
                spawner.currentCount++;
            }
        }).Run();
        outHits.Dispose();
    }
}
