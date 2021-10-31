using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Drawing;

public class PathDisplaySystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        var builder = DrawingManager.GetBuilder(true);

        this.Dependency = Entities.WithAll<AgentMovementData>().ForEach((ref DynamicBuffer<TrajectoryBufferElement> trajectory, in Translation translation, in PhysicsVelocity velocity, in AgentMovementData data) =>
        {
            // 射线检测可视化
            // float3 origin = translation.Value.addFloat2(math.normalizesafe(velocity.Linear.xz) * 0.26f);
            // builder.Ray(origin, math.down(), Color.red);

            // TODO:曲线平面化 颜色随速度深浅
            if (data.state == AgentState.Escape) trajectory.Add(translation.Value);
            // 轨迹可视化
            builder.PushLineWidth(2f);
            builder.Polyline(trajectory.Reinterpret<float3>().AsNativeArray(), Color.blue);
            builder.PopLineWidth();

        }).ScheduleParallel(this.Dependency);

        builder.DisposeAfter(this.Dependency);

        CompleteDependency();
    }
}