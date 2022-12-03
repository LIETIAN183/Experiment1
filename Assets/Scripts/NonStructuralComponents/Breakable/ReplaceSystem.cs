using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Havok.Physics;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct fluidInfo
{
    public float3 position;
    public quaternion rotation;
}
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
public partial class ReplaceSystem : SystemBase
{
    private EndFixedStepSimulationEntityCommandBufferSystem m_endFixedStepSimECBSystem;
    private StepPhysicsWorld stepPhysicsWorld;
    private PhysicsWorld physicsWorld;
    private List<Entity> deletedEntity;
    private Random random;

    public List<fluidInfo> fluidGeneratePositions;

    public GameObject fluidSolver;
    protected override void OnCreate()
    {
        m_endFixedStepSimECBSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
        deletedEntity = new List<Entity>();
        fluidGeneratePositions = new List<fluidInfo>();
        random = new Random();
        random.InitState((uint)System.DateTime.Now.Millisecond);
        this.Enabled = false;
    }

    protected override void OnStartRunning()
    {
        var ecb = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();

        Entities.WithAll<BreakableTag>().ForEach((in DynamicBuffer<EntityBufferElement> buffer) =>
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
                        ecb.AddComponent<OriginalState>(c, new OriginalState { originPosition = t.Value, originRotation = r.Value });
                        ecb.AddComponent<MCData>(c);
                    }
                }
            }
        }).Schedule();

        this.CompleteDependency();
    }

    public void RemoveAllFluid()
    {
        fluidSolver.GetComponent<GenerateFluid>().RemoveAllFluidInGo();
    }

    BufferFromEntity<Child> childList;
    BufferFromEntity<LinkedEntityGroup> linkedList;
    BufferFromEntity<EntityBufferElement> prefabList;
    ComponentDataFromEntity<Rotation> rotationList;
    ComponentDataFromEntity<Translation> translationList;
    ComponentDataFromEntity<PhysicsVelocity> velocityList;
    ComponentDataFromEntity<OriginalState> originStateList;
    ComponentDataFromEntity<BreakableTag> breakableList;

    [BurstCompile]
    protected override void OnUpdate()
    {
        var ecb = m_endFixedStepSimECBSystem.CreateCommandBuffer();

        childList = GetBufferFromEntity<Child>(true);
        linkedList = GetBufferFromEntity<LinkedEntityGroup>(true);
        prefabList = GetBufferFromEntity<EntityBufferElement>(true);
        breakableList = GetComponentDataFromEntity<BreakableTag>(true);
        rotationList = GetComponentDataFromEntity<Rotation>(true);
        translationList = GetComponentDataFromEntity<Translation>(true);
        velocityList = GetComponentDataFromEntity<PhysicsVelocity>(true);
        originStateList = GetComponentDataFromEntity<OriginalState>(true);

        // 判断使用的物理系统
        // UnityEngine.Debug.Log(World.GetOrCreateSystem<StepPhysicsWorld>().Simulation.GetType());
        // 如果使用 Unity Physics，选用注释的代码
        // var events = ((Simulation)World.GetOrCreateSystem<StepPhysicsWorld>().Simulation).CollisionEvents;

        var events = ((HavokSimulation)stepPhysicsWorld.Simulation).CollisionEvents;
        foreach (var e in events)
        {
            // 过滤事件，如果触发碰撞事件的双方都不是可破碎物体时跳过
            // var boolA = HasComponent<BreakableTag>(e.EntityA);
            var boolA = breakableList.HasComponent(e.EntityA);
            var boolB = breakableList.HasComponent(e.EntityB);
            if (!boolA && !boolB) continue;

            // 判断碰撞事件受力是否达到破碎阈值，破碎阈值为4f
            var force = e.CalculateDetails(ref physicsWorld).EstimatedImpulse;
            // if (force > 1f) UnityEngine.Debug.Log(force);
            // 碰撞体受到的力大于4f时，破碎
            if (force < 2f) continue;

            if (boolA && !deletedEntity.Contains(e.EntityA))
            {
                deletedEntity.Add(e.EntityA);
                replaceItem(e.EntityA, ecb);
            }

            if (boolB && !deletedEntity.Contains(e.EntityB))
            {
                deletedEntity.Add(e.EntityB);
                replaceItem(e.EntityB, ecb);
            }
        }
    }

    void replaceItem(Entity selectedEntity, EntityCommandBuffer ecb)
    {
        // 隐藏旧物体
        // 如果存在子物体，一同隐藏
        if (childList.HasComponent(selectedEntity))
        {
            foreach (var child in childList[selectedEntity].Reinterpret<Entity>())
            {
                ecb.AddComponent<Disabled>(child);
            }
        }
        ecb.AddComponent<Disabled>(selectedEntity);

        // 配置新物体
        // 随机挑选一个替换物
        var prefabs = prefabList[selectedEntity].Reinterpret<Entity>();
        var prefab = prefabs[random.NextInt(0, prefabs.Length)];

        // 配置替换物的位置、旋转角度和速度
        var targetRotation = rotationList[selectedEntity].Value;
        var targetTranslation = translationList[selectedEntity].Value;
        var targetVelocity = velocityList[selectedEntity];

        if (breakableList[selectedEntity].fluidInside)
        {
            fluidGeneratePositions.Add(new fluidInfo { position = targetTranslation, rotation = targetRotation });
        }
        // new float3x2(targetTranslation, ((Quaternion)targetRotation).eulerAngles));

        foreach (var child in linkedList[prefab].Reinterpret<Entity>())
        {
            if (originStateList.HasComponent(child))
            {
                var data = originStateList[child];
                // 计算每个子物体要替换的位置和旋转角度
                (float3 p, quaternion r) = rotateAroundPoint(float3.zero, targetRotation, data.originPosition, data.originRotation);
                // 设置位置、旋转角度和速度
                ecb.SetComponent<PhysicsVelocity>(child, targetVelocity);
                ecb.SetComponent<Translation>(child, new Translation { Value = p + targetTranslation });
                ecb.SetComponent<Rotation>(child, new Rotation { Value = r });
                ecb.SetComponent<MCData>(child, new MCData { previousVelinY = targetVelocity.Linear.y });
            }
        }
        // 生成新物体
        ecb.Instantiate(prefab);
    }

    // 物体绕点旋转
    public (float3 position, quaternion rotation) rotateAroundPoint(float3 pivot, quaternion targetRotation, float3 itemPosition, quaternion itemRotation)
    {
        // var temp = targetRotation - itemRotation;
        itemPosition = math.mul(targetRotation, itemPosition - pivot) + pivot;
        itemRotation = math.mul(targetRotation, itemRotation);
        return (itemPosition, itemRotation);
    }
}