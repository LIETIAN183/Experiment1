using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class WallBreakSystem : SystemBase
{
    BeginFixedStepSimulationEntityCommandBufferSystem m_beginFixedStepSimECBSystem;

    float timer;

    NativeList<Entity> breadkedList;

    protected override void OnCreate()
    {
        m_beginFixedStepSimECBSystem = World.GetExistingSystem<BeginFixedStepSimulationEntityCommandBufferSystem>();
        // wallbreaked = new NativeParallelHashMap<float3, Entity>(100, Allocator.Persistent);
        breadkedList = new NativeList<Entity>(100, Allocator.Persistent);
    }
    protected override void OnStartRunning()
    {
        var ecb = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();

        Entities.WithAll<WallBreak>().ForEach((in DynamicBuffer<EntityBufferElement> buffer) =>
        {
            var prefabs = buffer.Reinterpret<Entity>();
            foreach (var prefab in prefabs)
            {
                // 需要把替换物的预制体初始位置设置在原点，这样子物体的初始位置就是相对原点的偏移量，方便后续计算（这件事需要在一开始就设置好，即在编辑器中，未打包前）
                //分割后的替换物都有子物体
                var childbuffer = GetBuffer<LinkedEntityGroup>(prefab).Reinterpret<Entity>();

                foreach (var c in childbuffer)
                {
                    if (c == prefab) continue;
                    if (HasComponent<Translation>(c) && !HasComponent<OriginalState>(c))
                    {
                        var t = GetComponent<Translation>(c);
                        var r = GetComponent<Rotation>(c);
                        var mass = GetComponent<PhysicsMass>(c);
                        ecb.AddComponent<OriginalState>(c, new OriginalState { originPosition = t.Value, originRotation = r.Value, inverseInertia = mass.InverseInertia });
                        ecb.AddComponent<MCData>(c);
                        // ecb.AddComponent<PhysicsGravityFactor>(c, new PhysicsGravityFactor { Value = 0 });
                        // var mass = GetComponent<PhysicsMass>(c);
                        mass.InverseMass = 0;
                        mass.InverseInertia = float3.zero;
                        ecb.SetComponent<PhysicsMass>(c, mass);
                        ecb.AddComponent<WallBreaked>(c);
                        // ecb.RemoveComponent<PhysicsDamping>(c);
                    }
                }
            }
        }).Schedule();

        this.CompleteDependency();
    }
    protected override void OnUpdate()
    {
        var ecb = m_beginFixedStepSimECBSystem.CreateCommandBuffer();

        var linkList = GetBufferFromEntity<LinkedEntityGroup>(true);

        if (Input.GetKeyUp(KeyCode.F))
        {
            Entities.WithAll<WallBreak>().ForEach((Entity e, ref DynamicBuffer<EntityBufferElement> prefabs, in Translation translation, in Rotation rotation) =>
            {
                ecb.AddComponent<Disabled>(e);

                var prefabList = prefabs.Reinterpret<Entity>();
                var prefab = prefabList[0];

                foreach (var child in linkList[prefab].Reinterpret<Entity>())
                {
                    if (HasComponent<OriginalState>(child))
                    {
                        var data = GetComponent<OriginalState>(child);

                        var p = math.mul(rotation.Value, data.originPosition - float3.zero) + float3.zero;
                        var r = math.mul(rotation.Value, data.originRotation);

                        ecb.SetComponent<Translation>(child, new Translation { Value = p + translation.Value });
                        ecb.SetComponent<Rotation>(child, new Rotation { Value = r });
                        ecb.SetComponent<MCData>(child, new MCData { previousVelinY = 0 });
                    }
                }
                ecb.Instantiate(prefab);

            }).Schedule();
            this.CompleteDependency();
        }



        if (Input.GetKeyUp(KeyCode.G))
        {
            // UnityEngine.Debug.Log("Test");
            // if (timer < 0.5f)
            // {
            //     timer += Time.DeltaTime;
            //     return;
            // }
            // var keys = wallbreaked.GetKeyArray(AllocatorManager.Temp);
            // var temp = keys[0];
            // var entity = wallbreaked[temp];

            // var state = GetComponent<OriginalState>(entity);
            // var mass = GetComponent<PhysicsMass>(entity);
            // mass.InverseInertia = state.inverseInertia;
            // mass.InverseMass = 1;
            // ecb.SetComponent<PhysicsMass>(entity, mass);
            // wallbreaked.Remove(temp);

            // timer = 0;

            // var entity = breadkedList[0];
            // var state = GetComponent<OriginalState>(entity);
            // var mass = GetComponent<PhysicsMass>(entity);
            // mass.InverseInertia = state.inverseInertia;
            // mass.InverseMass = 1;
            // ecb.SetComponent<PhysicsMass>(entity, mass);
            // breadkedList.RemoveAt(0);
            var temp = 0;
            Entities.WithAll<WallBreaked>().WithoutBurst().ForEach((Entity e, ref PhysicsMass mass, in OriginalState state) =>
            {
                temp++;
                if (temp > 2) return;
                breadkedList.Add(e);
                ecb.RemoveComponent<WallBreaked>(e);

                mass.InverseMass = 1;
                mass.InverseInertia = state.inverseInertia;
                ecb.RemoveComponent<WallBreaked>(e);
            }).Run();

        }



        // UnityEngine.Debug.Log("Test1");
    }

}
