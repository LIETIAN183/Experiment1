using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Drawing;

[UpdateInGroup(typeof(AgentMovementSystemGroup))]
[BurstCompile]
public partial struct FlowFieldMovementSystem : ISystem
{
    private ComponentLookup<AgentMovementData> agentDataList;
    private ComponentLookup<LocalTransform> localTransformList;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        agentDataList = SystemAPI.GetComponentLookup<AgentMovementData>(true);
        localTransformList = SystemAPI.GetComponentLookup<LocalTransform>(true);
        state.Enabled = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();
        var deltaTime = SystemAPI.Time.DeltaTime;
        var cells = SystemAPI.GetSingletonBuffer<CellBuffer>(true).Reinterpret<CellData>().AsNativeArray();
        var des = SystemAPI.GetSingletonBuffer<DestinationBuffer>(true).Reinterpret<int>().AsNativeArray();
        var timerData = SystemAPI.GetSingleton<TimerData>();

        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        agentDataList.Update(ref state);
        localTransformList.Update(ref state);

        // var builder = DrawingManager.GetBuilder(true);
        var ecb = new EntityCommandBuffer(Allocator.Persistent);
        new OurModelJob
        {
            deltaTime = deltaTime,
            cells = cells,
            physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            settingData = setting,
            accData = timerData,
            // standardVel = Utilities.GetStandardVelByPGA(timerData.curPGA),
            standardVel = 3,
            agentDataList = agentDataList,
            localTransformList = localTransformList,
            parallelECB = ecb.AsParallelWriter(),
            dests = des,
            // builder = builder,
            randomInitSeed = (uint)(SystemAPI.GetSingleton<RandomSeed>().seed + SystemAPI.Time.ElapsedTime.GetHashCode()),
            variable = SystemAPI.GetSingleton<SimConfigData>().average
        }.ScheduleParallel(state.Dependency).Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        // var type = SystemAPI.GetSingleton<SimConfigData>().simType;
        // switch (type)
        // {
        //     case 0:
        //         state.Dependency = new BasicSFMJob
        //         {
        //             deltaTime = deltaTime,
        //             des = cells[des[0]].worldPos.xz,
        //             standardVel = Utilities.GetStandardVelByPGA(timerData.curPGA),
        //             physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld
        //         }.ScheduleParallel(state.Dependency);
        //         break;
        //     case 1:
        //         state.Dependency = new EarthquakeSFMJob
        //         {
        //             deltaTime = deltaTime,
        //             des = cells[des[0]].worldPos.xz,
        //             standardVel = Utilities.GetStandardVelByPGA(timerData.curPGA),
        //             accData = timerData,
        //             physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld
        //         }.ScheduleParallel(state.Dependency);
        //         break;
        //     case 2:
        //     case 3:

        //         break;
        //     default:
        //         break;
        // }


        // builder.Dispose();


        // if (setting.index == 0)
        // {
        //     // Global FlowField
        //     state.Dependency = new GlobalFlowFieldJob
        //     {
        //         cells = cells,
        //         settingData = setting
        //     }.ScheduleParallel(state.Dependency);
        // }
        // else if (setting.index == 1)
        // {// Basic SFM + Local FlowField
        //     state.Dependency = new BasicSFM_LocalFFJob
        //     {
        //         deltaTime = deltaTime,
        //         des = cells[des[0]].worldPos.xz,
        //         physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
        //         cells = cells,
        //         settingData = setting
        //     }.ScheduleParallel(state.Dependency);
        // }
    }
}
