using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class AgentStateSystem : SystemBase
{
    private float accInMenmory;
    protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        var destination = GetSingleton<FlowFieldSettingData>().destination;

        // 记录仿真以来的最大地震强度，以此来进行延迟时间的计算
        float acc = math.length(GetSingleton<AccTimerData>().acc);
        if (accInMenmory < acc) accInMenmory = acc;
        float accTemp = accInMenmory;
        float elapsedTime = GetSingleton<AccTimerData>().elapsedTime;

        Entities.ForEach((ref PhysicsVelocity velocity, ref AgentMovementData movementData, ref Translation translation) =>
        {
            switch (movementData.state)
            {
                case AgentState.NotActive:
                    return;
                case AgentState.Delay:
                    // 计算反应时间，超过反应时间后智能体开始撤离
                    if (movementData.reactionTimeVariable * 25 * math.exp(-accTemp) < elapsedTime)
                    {
                        movementData.state = AgentState.Escape;
                        movementData.reactionTime = movementData.reactionTimeVariable * 25 * math.exp(-accTemp);
                    }
                    return;
                case AgentState.Escape:
                    // 到达目的地，停止运动 距离目的地小于0.5f
                    if (math.length(destination.xz - translation.Value.xz) < 0.5f) movementData.state = AgentState.Escaped;
                    return;
                case AgentState.Escaped:
                    velocity.Linear = float3.zero;
                    translation.Value.y = -10;
                    return;
                default:
                    return;
            }
        }).ScheduleParallel();
    }
}
