using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateBefore(typeof(TimerSystem))]
[BurstCompile]
public partial struct SimControlSystem : ISystem
{
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
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {


        StartCheck(ref state);
        EndCheck(ref state);
    }

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
                message.displayType = 1;
                SystemAPI.SetSingleton(message);
                return;
            }

            // 重置事件状态
            startEvent.isActivate = false;
            SystemAPI.SetComponent<StartSeismicEvent>(state.SystemHandle, startEvent);

            var simulationSetting = SystemAPI.GetSingleton<SimConfigData>();
            var spawnerData = SystemAPI.GetSingleton<SpawnerData>();
            if (simulationSetting.simAgent && !spawnerData.canSpawn)
            {
                spawnerData.canSpawn = true;
                SystemAPI.SetSingleton(spawnerData);
                var message = SystemAPI.GetSingleton<MessageEvent>();
                message.isActivate = true;
                message.message = "Spawning Agents";
                message.displayType = 0;
                SystemAPI.SetSingleton(message);
            }

            // TODO: MultiRound Error
            SystemInit(ref state).Complete();
            // var initJob = new TimerInitJob
            // {
            //     eventIndex = 0,
            //     simPGA = 0,
            //     em = state.EntityManager
            // };
            // initJob.Schedule(state.Dependency).Complete();

            SubSystemManager(ref state, true);
        }
    }

    public void EndCheck(ref SystemState state)
    {
        var endEvent = SystemAPI.GetComponent<EndSeismicEvent>(state.SystemHandle);
        if (endEvent.isActivate)
        {
            // 重置事件状态
            endEvent.isActivate = false;
            SystemAPI.SetComponent<EndSeismicEvent>(state.SystemHandle, endEvent);

            // 关闭仿真子系统
            SubSystemManager(ref state, false);
        }
    }

    public JobHandle SystemInit(ref SystemState state)
    {
        var initJobDependency = new TimerInitJob
        {
            eventIndex = 0,
            simPGA = 0
        }.Schedule(state.Dependency);

        return initJobDependency;
    }

    private void SubSystemManager(ref SystemState state, bool enabled)
    {
        if (!SystemAPI.HasSingleton<SimConfigData>()) return;
        var setting = SystemAPI.GetSingleton<SimConfigData>();

        state.WorldUnmanaged.GetExistingSystemState<TimerSystem>().Enabled = true;

        if (setting.simEnvironment)
        {
            // 环境系统
            state.WorldUnmanaged.GetExistingSystemState<MCMotionSystem>().Enabled = enabled;
            state.WorldUnmanaged.GetExistingSystemState<FCOscSystem>().Enabled = enabled;

            if (setting.itemDestructible)
            {
                state.WorldUnmanaged.GetExistingSystemState<DestructionSystem>().Enabled = enabled;
            }
        }

        if (setting.simFlowField)
        {
            // 路径算法
            state.WorldUnmanaged.GetExistingSystemState<FlowFieldSystem>().Enabled = enabled;
        }

        if (setting.simAgent)
        {
            // 行人状态切换
            state.World.GetExistingSystemManaged<SeismicActiveSystem>().Enabled = enabled;
            state.World.GetExistingSystemManaged<CheckReachedDestinationSystem>().Enabled = enabled;

            // 人群算法
            state.World.GetExistingSystemManaged<AgentMovementSystem>().Enabled = enabled;
            // simulation.GetExistingSystemManaged<SFMmovementSystem>().Enabled = state;
            // simulation.GetExistingSystemManaged<SFMmovementSystem2>().Enabled = state;
            // simulation.GetExistingSystemManaged<SFMmovementSystem3>().Enabled = state;

            if (setting.displayTrajectories)
            {
                // 行人轨迹记录
                state.World.GetExistingSystemManaged<TrajectoryRecordSystem>().Enabled = enabled;
            }
        }

        if (setting.performStatistics)
        {
            // 统计系统
            state.World.GetExistingSystemManaged<SingleStatisticSystem>().Enabled = enabled;
            state.World.GetExistingSystemManaged<RecordSystem>().Enabled = enabled;
        }
    }
}
