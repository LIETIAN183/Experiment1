using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Drawing;
using Unity.Burst;

[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(FlowFieldVisulizeSystem))]
[BurstCompile]
public partial struct TrajectoryDisplaySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) => state.Enabled = false;
    // [BurstCompile]
    // DrawingManager.GetBuilder 为 managed mathod, 不可 BurstCompile
    public void OnUpdate(ref SystemState state)
    {
        var builder = DrawingManager.GetBuilder(true);


        var drawJob = new DrawTrajectoriesJob
        {
            builder = builder
        }.ScheduleParallel(state.Dependency);


        builder.DisposeAfter(drawJob);
        drawJob.Complete();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}

[BurstCompile]
[WithAll(typeof(AgentMovementData)), WithAny(typeof(Escaping), typeof(Escaped)), WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
partial struct DrawTrajectoriesJob : IJobEntity
{
    public CommandBuilder builder;
    public void Execute(in DynamicBuffer<PosBuffer> posList)
    {
        // PushLineWidth 在 Job 外使用不生效
        builder.PushLineWidth(4f);
        builder.Polyline(posList.Reinterpret<float3>().AsNativeArray(), Color.white);
        builder.PopLineWidth();
    }
}