using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(TrajectoryRecordSystemGroup))]
[BurstCompile]
public partial struct TrajectoryRecordSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) => state.Enabled = false;
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        new CleanTrajectoriesJob().ScheduleParallel(state.Dependency).Complete();

        state.WorldUnmanaged.GetExistingSystemState<TrajectoryDisplaySystem>().Enabled = true;
    }
    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        state.WorldUnmanaged.GetExistingSystemState<TrajectoryDisplaySystem>().Enabled = false;
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        state.Dependency = new RecordTrajectoriesJob().ScheduleParallel(state.Dependency);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData))]
partial struct CleanTrajectoriesJob : IJobEntity
{
    void Execute(ref DynamicBuffer<PosBuffer> posList)
    {
        posList.Clear();
    }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData), typeof(Escaping))]
partial struct RecordTrajectoriesJob : IJobEntity
{
    void Execute(ref DynamicBuffer<PosBuffer> posList, in LocalTransform localTransform)
    {
        posList.Add(new float3(localTransform.Position.x, 1, localTransform.Position.z));
    }
}
