using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Physics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateBefore(typeof(TimerSystem))]
[BurstCompile]
public partial struct SimControlSystem : ISystem
{
    private bool screenShotFlag;
    private float screenShotTimer;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.AddComponentData<StartSeismicEvent>(state.SystemHandle, new StartSeismicEvent { isActivate = false });
        state.EntityManager.AddComponentData<EndSeismicEvent>(state.SystemHandle, new EndSeismicEvent { isActivate = false });
        state.EntityManager.AddComponentData<SimConfigData>(state.SystemHandle, new SimConfigData
        {
            simEnvironment = true,
            itemDestructible = true,
            simFlowField = true,
            simAgent = false,
            displayTrajectories = false,
            performStatistics = false
        });
        screenShotFlag = false;
        screenShotTimer = 0;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        StartCheck(ref state);
        EndCheck(ref state);
    }
    [BurstCompile]
    public void StartCheck(ref SystemState state)
    {
        var startEvent = SystemAPI.GetComponent<StartSeismicEvent>(state.SystemHandle);
        if (startEvent.isActivate)
        {
            // 检测数据读取是否成功，若失败则报错
            if (!SystemAPI.GetSingleton<DataLoadStateData>().isLoadSuccessed)
            {
                var message = SystemAPI.GetSingleton<MessageEvent>();
                message.isActivate = true;
                message.message = "Data Read Error, Cant Exceed Simulation";
                message.displayForever = true;
                SystemAPI.SetSingleton(message);
                return;
            }

            // 重置事件状态
            startEvent.isActivate = false;
            SystemAPI.SetComponent<StartSeismicEvent>(state.SystemHandle, startEvent);

            var simulationSetting = SystemAPI.GetSingleton<SimConfigData>();
            var spawnerData = SystemAPI.GetSingleton<SpawnerData>();
            if (simulationSetting.simAgent && spawnerData.currentCount < spawnerData.desireCount)
            {
                // 这里需要 Allocator.Persistent，因为生成 Agent 需要花费较多时间， Allocator.TempJob 时间不够
                var ecb = new EntityCommandBuffer(Allocator.Persistent);

                new SpawnerJob
                {
                    ecb = ecb,
                    physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                    randomInitSeed = (uint)(SystemAPI.GetSingleton<RandomSeed>().seed + SystemAPI.Time.ElapsedTime.GetHashCode()),
                }.Schedule(state.Dependency).Complete();
                ecb.Playback(state.EntityManager);
                ecb.Dispose();

                var message = SystemAPI.GetSingleton<MessageEvent>();
                message.isActivate = true;
                message.message = "Spawning Agents";
                message.displayForever = false;
                SystemAPI.SetSingleton(message);
            }

            SystemInit(ref state, startEvent.index, startEvent.targetPGA).Complete();

            SubSystemManager(ref state, true);

        }
    }
    // [BurstCompile]
    public void EndCheck(ref SystemState state)
    {
        var endEvent = SystemAPI.GetComponent<EndSeismicEvent>(state.SystemHandle);
        // 截屏延时后，结束本轮仿真
        if (screenShotFlag)
        {
            screenShotTimer -= SystemAPI.Time.DeltaTime;
            if (screenShotTimer < 0)
            {
                screenShotFlag = false;
                screenShotTimer = 0;
                // 重置事件状态
                endEvent.isActivate = false;
                SystemAPI.SetComponent<EndSeismicEvent>(state.SystemHandle, endEvent);

                // 关闭仿真子系统
                SubSystemManager(ref state, false);
            }
            return;
        }

        if (endEvent.isActivate)
        {
            // 截图
            var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();

            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/PedestrianDir" + setting.agentIndex + ".png");
            screenShotFlag = true;
            screenShotTimer = 0.5f;

        }
    }

    [BurstCompile]
    public JobHandle SystemInit(ref SystemState state, int eventIndex, float simPGA)
    {
        var initJobDependency = new TimerInitJob
        {
            eventIndex = eventIndex,
            simPGA = simPGA
        }.Schedule(state.Dependency);

        return initJobDependency;
    }

    [BurstCompile]
    private void SubSystemManager(ref SystemState state, bool enabled)
    {
        var unmanagedWorld = state.WorldUnmanaged;
        unmanagedWorld.GetExistingSystemState<TimerSystem>().Enabled = enabled;

        if (!SystemAPI.TryGetSingleton<SimConfigData>(out var setting)) return;

        if (setting.simEnvironment)
        {
            // 环境系统
            unmanagedWorld.GetExistingSystemState<MCMotionSystem>().Enabled = enabled;
            unmanagedWorld.GetExistingSystemState<FCOscSystem>().Enabled = enabled;

            if (setting.itemDestructible)
            {
                unmanagedWorld.GetExistingSystemState<DestructionSystem>().Enabled = enabled;
            }
        }

        // 路径算法
        // 因为计算需要时间，不然路径计算会比其他系统晚一定时间，因此需要提前开启
        // 所以只有在不需要路径算法的时候，关闭即可
        unmanagedWorld.GetExistingSystemState<FlowFieldSystem>().Enabled = setting.simFlowField;

        if (setting.simAgent)
        {
            // 行人状态切换
            unmanagedWorld.GetExistingSystemState<AgentStateChangeSystem>().Enabled = enabled;
            // 人群算法
            // 和多轮仿真的恢复 Job 相关联，需要同步修改
            // state.World.GetExistingSystemManaged<AgentMovementSystem>().Enabled = enabled;
            unmanagedWorld.GetExistingSystemState<FlowFieldMovementSystem>().Enabled = enabled;
            // simulation.GetExistingSystemManaged<SFMmovementSystem>().Enabled = state;
            // simulation.GetExistingSystemManaged<SFMmovementSystem2>().Enabled = state;
            // simulation.GetExistingSystemManaged<SFMmovementSystem3>().Enabled = state;

            if (setting.displayTrajectories)
            {
                // 行人轨迹记录
                unmanagedWorld.GetExistingSystemState<TrajectoryRecordSystem>().Enabled = enabled;
            }
        }

        if (setting.performStatistics)
        {
            // 统计系统
            unmanagedWorld.GetExistingSystemState<SingleStatisticSystem>().Enabled = enabled;
            unmanagedWorld.GetExistingSystemState<RecordSystem>().Enabled = enabled;
        }
    }
}
