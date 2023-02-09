using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Havok.Physics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct DestructionSystem : ISystem, ISystemStartStop
{
    // 同一个物体可能存在多个碰撞事件，因此声音可以多个，但是替换只能同一个
    // 因此需要一个列表来检测有无重复
    private NativeList<Entity> deletedEntity;
    private Random random;

    // 辅助查询变量
    private BufferLookup<Child> childList;
    private BufferLookup<LinkedEntityGroup> linkedList;
    private BufferLookup<ReplacePrefabsBuffer> prefabList;
    private ComponentLookup<LocalTransform> localTransformList;
    private ComponentLookup<PhysicsVelocity> physicsVelocityList;
    private ComponentLookup<OriginPos_RotInfo> orgInfoList;
    private ComponentLookup<DCData> dcComponentList;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        childList = SystemAPI.GetBufferLookup<Child>(true);
        linkedList = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true);
        prefabList = SystemAPI.GetBufferLookup<ReplacePrefabsBuffer>(true);
        localTransformList = SystemAPI.GetComponentLookup<LocalTransform>(true);
        physicsVelocityList = SystemAPI.GetComponentLookup<PhysicsVelocity>(true);
        orgInfoList = SystemAPI.GetComponentLookup<OriginPos_RotInfo>(true);
        dcComponentList = SystemAPI.GetComponentLookup<DCData>(true);

        deletedEntity = new NativeList<Entity>(Allocator.Persistent);

        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.SetName(entity, "FluidInfoEntity");
        state.EntityManager.AddBuffer<FluidInfoBuffer>(entity);
        state.EntityManager.AddComponentData<ClearFluidEvent>(entity, new ClearFluidEvent { isActivate = false });
        // 不添加到 System Entity，因为 EntityQuery 查询不到
        // state.EntityManager.AddComponentData<ClearFluidEvent>(state.SystemHandle, new ClearFluidEvent { isActivate = false });

        random = new Random();
        random.InitState();
        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        linkedList.Update(ref state);
        physicsVelocityList.Update(ref state);
        localTransformList.Update(ref state);
        orgInfoList.Update(ref state);

        var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        var prefabJob = new DCPrefabsInitialize
        {
            linkedList = linkedList,
            physicsVelocityList = physicsVelocityList,
            localTransformList = localTransformList,
            orgInfoList = orgInfoList,
            ecb = ecb,
        }.Schedule(state.Dependency);

        prefabJob.Complete();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 重要，保证前置 Job 完成，不发生冲突
        state.EntityManager.CompleteDependencyBeforeRW<PhysicsVelocity>();
        state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
        state.EntityManager.CompleteDependencyBeforeRW<LocalTransform>();

        childList.Update(ref state);
        linkedList.Update(ref state);
        prefabList.Update(ref state);
        localTransformList.Update(ref state);
        physicsVelocityList.Update(ref state);
        orgInfoList.Update(ref state);
        dcComponentList.Update(ref state);

        var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        var havokSimulation = simulation.AsHavokSimulation();
        HavokCollisionEvents events = havokSimulation.CollisionEvents;
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        foreach (var e in events)
        {
            // 过滤事件，如果触发碰撞事件的双方都不是可破碎物体时跳过
            var boolA = dcComponentList.HasComponent(e.EntityA);
            var boolB = dcComponentList.HasComponent(e.EntityB);
            if (!boolA && !boolB) continue;

            // 判断碰撞事件受力是否达到破碎阈值，破碎阈值为4f
            var force = e.CalculateDetails(ref physicsWorld).EstimatedImpulse;
            // 碰撞体受到的力大于4f时，破碎
            if (force < 2f) continue;

            if (boolA && !deletedEntity.Contains(e.EntityA))
            {
                deletedEntity.Add(e.EntityA);
                replaceEntity(e.EntityA, ecb);
            }

            if (boolB && !deletedEntity.Contains(e.EntityB))
            {
                deletedEntity.Add(e.EntityB);
                replaceEntity(e.EntityB, ecb);
            }
        }
    }

    [BurstCompile]
    public void replaceEntity(Entity sourceEntity, EntityCommandBuffer ecb)
    {
        // 隐藏旧物体
        // 如果存在子物体，一同隐藏
        if (childList.HasBuffer(sourceEntity))
        {
            foreach (var child in childList[sourceEntity].Reinterpret<Entity>())
            {
                ecb.AddComponent<Disabled>(child);
            }
        }
        ecb.AddComponent<Disabled>(sourceEntity);

        // 配置新物体
        // 随机挑选一个替换物
        var CandidateEntities = prefabList[sourceEntity].Reinterpret<Entity>();
        // var prefab = prefabs[random.NextInt(0, prefabs.Length)];
        var targetEntity = CandidateEntities[random.NextInt(0, CandidateEntities.Length)];

        // 配置替换物的位置、旋转角度和速度
        var targetRot = localTransformList[sourceEntity].Rotation;
        var targetPos = localTransformList[sourceEntity].Position;
        var targetVelocity = physicsVelocityList[sourceEntity];

        if (dcComponentList[sourceEntity].fluidInside)
        {
            var fluidInfoBuffer = SystemAPI.GetSingletonBuffer<FluidInfoBuffer>();
            fluidInfoBuffer.Add(new fluidInfo { position = targetPos, rotation = targetRot });
        }

        foreach (var child in linkedList[targetEntity].Reinterpret<Entity>())
        {
            if (orgInfoList.HasComponent(child))
            {
                var data = orgInfoList[child];
                // 计算每个子物体要替换的位置和旋转角度
                (float3 p, quaternion r) = rotateAroundPoint(float3.zero, targetRot, data.orgPos, data.orgRot);
                // 设置位置、旋转角度和速度
                ecb.SetComponent<PhysicsVelocity>(child, targetVelocity);
                ecb.SetComponent<LocalTransform>(child, LocalTransform.FromPositionRotation(p + targetPos, r));
                ecb.SetComponent<MCData>(child, new MCData { preVelinY = targetVelocity.Linear.y });
            }
        }
        // 生成新物体
        ecb.Instantiate(targetEntity);
    }

    // 物体绕点旋转
    [BurstCompile]
    public (float3 position, quaternion rotation) rotateAroundPoint(float3 pivot, quaternion targetRotation, float3 itemPosition, quaternion itemRotation)
    {
        itemPosition = math.mul(targetRotation, itemPosition - pivot) + pivot;
        itemRotation = math.mul(targetRotation, itemRotation);
        return (itemPosition, itemRotation);
    }
}

// 不能用 SchedualParallel，因为需要对应 entity 的 sortKey， 可是子物体和获得的 sortKey 的父物体不是同一个物体
[BurstCompile]
partial struct DCPrefabsInitialize : IJobEntity
{
    [ReadOnly] public BufferLookup<LinkedEntityGroup> linkedList;
    [ReadOnly] public ComponentLookup<PhysicsVelocity> physicsVelocityList;
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformList;
    [ReadOnly] public ComponentLookup<OriginPos_RotInfo> orgInfoList;
    public EntityCommandBuffer ecb;
    void Execute(in DynamicBuffer<ReplacePrefabsBuffer> buffer)
    {
        // var prefabs = buffer.Reinterpret<Entity>();
        foreach (var curPrefab in buffer.Reinterpret<Entity>())
        {
            // 获得当前 Prefab 的每个子元素
            // 遍历，对物理子元素，存储初始位置和旋转角度。
            // 因为该子元素加入物理计算，因此会与父物体脱离父子关系，所以生成的时候需要手动设置生成的位置，因此需要初始信息
            foreach (var child in linkedList[curPrefab].Reinterpret<Entity>())
            {
                if (child == curPrefab) continue;
                if (physicsVelocityList.HasComponent(child) && !orgInfoList.HasComponent(child))
                {
                    var source = localTransformList[child];
                    ecb.AddComponent<OriginPos_RotInfo>(child, new OriginPos_RotInfo { orgPos = source.Position, orgRot = source.Rotation });
                    // ecb.AddComponent<OriginPos_RotInfo>(child, new OriginPos_RotInfo { orgPos = new float3(1, 1, 1), orgRot = source.Rotation });
                    ecb.AddComponent<MCData>(child);
                }
            }
        }
    }
}