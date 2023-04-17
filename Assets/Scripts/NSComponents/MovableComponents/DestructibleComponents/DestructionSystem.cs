using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [UpdateAfter(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct DestructionSystem : ISystem, ISystemStartStop
{
    // 同一个物体可能存在多个碰撞事件，因此声音可以多个，但是替换只能同一个
    // 因此需要一个列表来检测有无重复
    private NativeList<Entity> deletedEntity;
    private Random random;

    // 辅助查询变量
    // FluidInfoBuffer 只存在一个
    private Entity fluidEntity;
    private BufferLookup<FluidInfoBuffer> fluidInfoBuffer;
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
        fluidInfoBuffer = SystemAPI.GetBufferLookup<FluidInfoBuffer>();
        localTransformList = SystemAPI.GetComponentLookup<LocalTransform>(true);
        physicsVelocityList = SystemAPI.GetComponentLookup<PhysicsVelocity>(true);
        orgInfoList = SystemAPI.GetComponentLookup<OriginPos_RotInfo>(true);
        dcComponentList = SystemAPI.GetComponentLookup<DCData>(true);

        deletedEntity = new NativeList<Entity>(Allocator.Persistent);

        fluidEntity = state.EntityManager.CreateEntity();
        state.EntityManager.SetName(fluidEntity, "FluidInfoEntity");
        // 传输要生产的流体位置
        state.EntityManager.AddBuffer<FluidInfoBuffer>(fluidEntity);
        state.EntityManager.AddComponentData<ClearFluidEvent>(fluidEntity, new ClearFluidEvent { isActivate = false });
        // 传输世界中已有流体的位置
        state.EntityManager.AddBuffer<Pos2DBuffer>(fluidEntity);
        // 不添加到 System Entity，因为 EntityQuery 查询不到
        // state.EntityManager.AddComponentData<ClearFluidEvent>(state.SystemHandle, new ClearFluidEvent { isActivate = false });

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

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        state.Dependency = new DCPrefabsInitialize
        {
            linkedList = linkedList,
            physicsVelocityList = physicsVelocityList,
            localTransformList = localTransformList,
            orgInfoList = orgInfoList,
            ecb = ecb,
        }.Schedule(state.Dependency);

        // 保证在替换前，完成 Component 的添加
        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        random = Random.CreateFromIndex((uint)(SystemAPI.GetSingleton<RandomSeed>().seed + SystemAPI.Time.ElapsedTime.GetHashCode()));
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
        fluidInfoBuffer.Update(ref state);
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
            // 碰撞体受到的力大于15f时，破碎
            // UnityEngine.Debug.Log(force);
            if (force < 10f) continue;

            if (boolA && !deletedEntity.Contains(e.EntityA))
            {
                deletedEntity.Add(e.EntityA);
                state.Dependency = new DCReplaceJob
                {
                    childList = childList,
                    prefabList = prefabList,
                    linkedList = linkedList,
                    physicsVelocityList = physicsVelocityList,
                    localTransformList = localTransformList,
                    orgInfoList = orgInfoList,
                    dcComponentList = dcComponentList,
                    seed = random.NextUInt(),
                    fluidEntity = fluidEntity,
                    fluidInfoBuffer = fluidInfoBuffer,
                    replacedEntity = e.EntityA,
                    ecb = ecb,
                }.Schedule(state.Dependency);
            }

            if (boolB && !deletedEntity.Contains(e.EntityB))
            {
                deletedEntity.Add(e.EntityB);
                state.Dependency = new DCReplaceJob
                {
                    childList = childList,
                    prefabList = prefabList,
                    linkedList = linkedList,
                    physicsVelocityList = physicsVelocityList,
                    localTransformList = localTransformList,
                    orgInfoList = orgInfoList,
                    dcComponentList = dcComponentList,
                    seed = random.NextUInt(),
                    fluidEntity = fluidEntity,
                    fluidInfoBuffer = fluidInfoBuffer,
                    replacedEntity = e.EntityB,
                    ecb = ecb,
                }.Schedule(state.Dependency);
            }
        }
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
                    ecb.AddComponent<MCData>(child);
                }
            }
        }
    }
}

[BurstCompile]
partial struct DCReplaceJob : IJob
{
    [ReadOnly] public BufferLookup<Child> childList;
    [ReadOnly] public BufferLookup<ReplacePrefabsBuffer> prefabList;
    [ReadOnly] public BufferLookup<LinkedEntityGroup> linkedList;
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformList;
    [ReadOnly] public ComponentLookup<DCData> dcComponentList;
    [ReadOnly] public ComponentLookup<OriginPos_RotInfo> orgInfoList;
    [ReadOnly] public ComponentLookup<PhysicsVelocity> physicsVelocityList;
    [ReadOnly] public uint seed;

    [ReadOnly] public Entity fluidEntity;
    public BufferLookup<FluidInfoBuffer> fluidInfoBuffer;

    public Entity replacedEntity;
    public EntityCommandBuffer ecb;

    public void Execute()
    {
        // 隐藏旧物体
        // 如果存在子物体，一同隐藏
        if (childList.HasBuffer(replacedEntity))
        {
            ecb.AddComponent<Disabled>(childList[replacedEntity].Reinterpret<Entity>().AsNativeArray());
        }
        ecb.AddComponent<Disabled>(replacedEntity);

        // 配置新物体
        // 随机挑选一个替换物
        var CandidateEntities = prefabList[replacedEntity].Reinterpret<Entity>();
        var random = new Random(seed);
        // var targetEntity = CandidateEntities[random.NextInt(0, CandidateEntities.Length)];
        var targetEntity = CandidateEntities[1];
        // 获取替换物的位置、旋转角度和速度
        var targetRot = localTransformList[replacedEntity].Rotation;
        var targetPos = localTransformList[replacedEntity].Position;
        var targetVelocity = physicsVelocityList[replacedEntity];

        if (dcComponentList[replacedEntity].fluidInside)
        {
            fluidInfoBuffer[fluidEntity].Add(new fluidInfo { position = targetPos, rotation = targetRot });
        }

        foreach (var child in linkedList[targetEntity].Reinterpret<Entity>())
        {
            if (child == targetEntity) continue;
            if (orgInfoList.HasComponent(child))
            {
                var data = orgInfoList[child];
                // 计算每个子物体要替换的位置和旋转角度
                var (p, r) = Utilities.rotateAroundPoint(float3.zero, targetRot, data.orgPos, data.orgRot);
                // 不能直接生成只有物理元素的子组件，因为有可能存在渲染子组件
                // var entity = ecb.Instantiate(child);
                // 设置位置、旋转角度和速度
                ecb.SetComponent<LocalTransform>(child, LocalTransform.FromPositionRotation(p + targetPos, r));
                ecb.SetComponent<PhysicsVelocity>(child, targetVelocity);
                ecb.SetComponent<MCData>(child, new MCData { preVelinY = targetVelocity.Linear.y });
            }
        }
        // 生成新物体
        ecb.Instantiate(targetEntity);
    }
}