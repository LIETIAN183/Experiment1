using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;
using Drawing;

// [DisableAutoCreation]
public class PathDisplaySystem : SystemBase
{
    // protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        var builder = DrawingManager.GetBuilder(true);

        this.Dependency = Entities.WithAll<AgentMovementData>().ForEach((ref DynamicBuffer<TrajectoryBufferElement> trajectory, ref AgentMovementData data, in Translation translation, in PhysicsVelocity velocity) =>
        {
            // 射线检测可视化
            // float3 origin = translation.Value.addFloat2(math.normalizesafe(velocity.Linear.xz) * 0.26f);
            // builder.Ray(origin, math.down(), Color.red);

            // TODO:曲线平面化 颜色随速度深浅
            if (data.state == AgentState.Escape)
            {
                data.pathLength += math.distance(data.lastPosition, translation.Value);
                trajectory.Add(translation.Value);
                data.lastPosition = translation.Value;
            }
            // 轨迹可视化
            builder.PushLineWidth(2f);
            builder.Polyline(trajectory.Reinterpret<float3>().AsNativeArray(), Color.blue);
            builder.PopLineWidth();

        }).ScheduleParallel(this.Dependency);

        builder.DisposeAfter(this.Dependency);

        CompleteDependency();
    }
}