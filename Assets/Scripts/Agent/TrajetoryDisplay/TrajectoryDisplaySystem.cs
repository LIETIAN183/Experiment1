using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;
using Drawing;

public partial class TrajectoryDisplaySystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        var builder = DrawingManager.GetBuilder(true);

        this.Dependency = Entities.WithAny<Escaping, Escaped>().WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled).ForEach((in DynamicBuffer<TrajectoryBufferElement> trajectory) =>
        {
            // 轨迹可视化
            builder.PushLineWidth(2f);
            builder.Polyline(trajectory.Reinterpret<float3>().AsNativeArray(), Color.white);
            builder.PopLineWidth();
        }).ScheduleParallel(this.Dependency);

        builder.DisposeAfter(this.Dependency);

        CompleteDependency();
    }
}