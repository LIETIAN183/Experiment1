using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
// using UnityEngine;
using Havok.Physics;
using System.Collections.Generic;

public partial class ReplaceSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimECBSystem;
    private StepPhysicsWorld stepPhysicsWorld;

    private List<Entity> deletedEntity;

    private int waitActiveNumber;
    protected override void OnCreate()
    {
        m_BeginSimECBSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        waitActiveNumber = 0;
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        deletedEntity = new List<Entity>();
    }

    protected override void OnStartRunning()
    {
        var ecb = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();

        Entities.WithAll<BreakableTag>().ForEach((in DynamicBuffer<EntityBufferElement> buffer) =>
        {
            var prefabs = buffer.Reinterpret<Entity>();
            foreach (var prefab in prefabs)
            {
                // 需要把替换物的初始位置设置在原点，这样子物体的初始位置就是相对原点的偏移量，方便后续计算
                ecb.SetComponent<Translation>(prefab, new Translation { Value = float3.zero });

                //分割后的替换物都有子物体
                var childbuffer = GetBuffer<LinkedEntityGroup>(prefab);

                foreach (var c in childbuffer)
                {
                    if (HasComponent<Translation>(c.Value) && !HasComponent<OriginalInformation>(c.Value))
                    {
                        var t = GetComponent<Translation>(c.Value);
                        var r = GetComponent<Rotation>(c.Value);
                        ecb.AddComponent<OriginalInformation>(c.Value, new OriginalInformation { originPosition = t.Value, originRotation = r.Value });
                    }
                }
            }

        }).Schedule();

    }

    protected override void OnUpdate()
    {
        var childList = GetBufferFromEntity<Child>(true);
        var ecb = m_BeginSimECBSystem.CreateCommandBuffer();
        var random = new Random();
        random.InitState((uint)System.DateTime.Now.Millisecond);
        // 判断使用的物理系统
        // UnityEngine.Debug.Log(World.GetOrCreateSystem<StepPhysicsWorld>().Simulation.GetType());
        // 如果使用 Unity Physics，选用注释的代码
        // var events = ((Simulation)World.GetOrCreateSystem<StepPhysicsWorld>().Simulation).CollisionEvents;
        var events = ((HavokSimulation)stepPhysicsWorld.Simulation).CollisionEvents;
        // World.GetOrCreateSystem<StepPhysicsWorld>()
        int i = 0;
        foreach (var e in events)
        {
            var selectedEntity = HasComponent<BreakableTag>(e.EntityA) ? e.EntityA : e.EntityB;
            if (deletedEntity.Contains(selectedEntity))
            {
                i++;
                continue;
            }
            else
            {
                deletedEntity.Add(selectedEntity);
            }

            if (childList.HasComponent(selectedEntity))
            {
                foreach (var c in childList[selectedEntity])
                {
                    // 隐藏旧物体
                    ecb.AddComponent<Disabled>(c.Value);
                }
                var prefabs = GetBuffer<EntityBufferElement>(selectedEntity).Reinterpret<Entity>();
                ecb.AddComponent<Disabled>(selectedEntity);
                var index = random.NextInt(0, prefabs.Length);
                var prefab = prefabs[index];
                var childbuffer = GetBuffer<LinkedEntityGroup>(prefab);
                var rotation = GetComponent<Rotation>(selectedEntity);
                var translation = GetComponent<Translation>(selectedEntity);
                foreach (var c in childbuffer)
                {
                    if (HasComponent<OriginalInformation>(c.Value))
                    {
                        var data = GetComponent<OriginalInformation>(c.Value);

                        float3 p;
                        quaternion r;
                        (p, r) = rotateAroundPoint(float3.zero, rotation.Value, data.originPosition, data.originRotation);
                        ecb.SetComponent<Translation>(c.Value, new Translation { Value = p + translation.Value });
                        ecb.SetComponent<Rotation>(c.Value, new Rotation { Value = r });
                        // todo:替换子物体的运动速度
                    }
                }
                ecb.Instantiate(prefab);
            }
        }
        //     foreach (var event in events){
        //     // i++;
        // }
        UnityEngine.Debug.Log(i);


        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.R))
        {




            var replace = Entities.WithAll<BreakableTag>().WithReadOnly(childList).ForEach((Entity e, in BreakableTag breakableTag, in DynamicBuffer<EntityBufferElement> buffer, in Translation translation, in Rotation rotation) =>
            {
                // 判断有无子物体
                if (childList.HasComponent(e))
                {
                    foreach (var c in childList[e])
                    {
                        // 隐藏旧物体
                        ecb.AddComponent<Disabled>(c.Value);
                    }
                }
                var prefabs = buffer.Reinterpret<Entity>();
                ecb.AddComponent<Disabled>(e);
                // 生成新的替换物并修改相应子物体的位置和旋转角度
                // 如果替换后出现弹跳碰撞的现象，那是因为子物体的碰撞体存在重叠，所以替换的瞬间就子物体之间弹开分离
                var index = random.NextInt(0, prefabs.Length);
                var prefab = prefabs[index];
                var childbuffer = GetBuffer<LinkedEntityGroup>(prefab);
                foreach (var c in childbuffer)
                {
                    if (HasComponent<OriginalInformation>(c.Value))
                    {
                        var data = GetComponent<OriginalInformation>(c.Value);
                        float3 p;
                        quaternion r;
                        (p, r) = rotateAroundPoint(float3.zero, rotation.Value, data.originPosition, data.originRotation);
                        ecb.SetComponent<Translation>(c.Value, new Translation { Value = p + translation.Value });
                        ecb.SetComponent<Rotation>(c.Value, new Rotation { Value = r });
                        // todo:替换子物体的运动速度
                    }
                }
                ecb.Instantiate(prefab);
            }).Schedule(Dependency);

            replace.Complete();
        }
    }

    [BurstCompile]
    struct CollisionEventTriggerJob : ICollisionEventsJob
    {

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
        }
    }


    // 物体绕点旋转
    public static (float3 position, quaternion rotation) rotateAroundPoint(float3 pivot, quaternion targetRotation, float3 itemPosition, quaternion itemRotation)
    {
        // var temp = targetRotation - itemRotation;
        itemPosition = math.mul(targetRotation, itemPosition - pivot) + pivot;
        itemRotation = math.mul(targetRotation, itemRotation);

        return (itemPosition, itemRotation);
    }
}