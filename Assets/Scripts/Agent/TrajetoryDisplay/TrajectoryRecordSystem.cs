using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(TrajectoryRecordSystemGroup))]
public partial class TrajectoryRecordSystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;

    protected override void OnStartRunning()
    {
        // 放到AnalysisCircyleSystem中重置，否则Display系统会没等初始化重置就显示上一次的轨迹
        Entities.WithAll<AgentMovementData>().ForEach((ref DynamicBuffer<TrajectoryBufferElement> trajectory) =>
        {
            trajectory.Clear();
        }).ScheduleParallel();
        // 初始化完成后才能开始下一步
        this.CompleteDependency();
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<TrajectoryDisplaySystem>().Enabled = true;
    }
    protected override void OnStopRunning()
    {
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<TrajectoryDisplaySystem>().Enabled = false;
    }
    protected override void OnUpdate()
    {
        Entities.WithAll<Escaping>().ForEach((ref DynamicBuffer<TrajectoryBufferElement> trajectory, in Translation translation) =>
        {
            var temp = translation.Value;
            temp.y = 1;
            trajectory.Add(temp);
        }).Schedule();
    }
}
