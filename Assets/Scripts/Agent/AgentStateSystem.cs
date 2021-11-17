using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class AgentStateSystem : SystemBase
{
    // protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        var timer = GetSingleton<AccTimerData>();
        // 记录仿真以来的最大地震强度，以此来进行延迟时间的计算
        float _pga = timer.pga;
        float elapsedTime = timer.elapsedTime;

        Entities.WithAll<AgentMovementData>().ForEach((ref PhysicsVelocity velocity, ref AgentMovementData movementData, ref Translation translation) =>
        {
            switch (movementData.state)
            {
                case AgentState.NotActive:
                    velocity.Linear = float3.zero;
                    return;
                case AgentState.Delay:
                    // 计算反应时间，超过反应时间后智能体开始撤离
                    if (movementData.reactionTimeVariable * 25 * math.exp(-_pga) < elapsedTime)
                    {
                        movementData.state = AgentState.Escape;
                        movementData.reactionTime = movementData.reactionTimeVariable * 25 * math.exp(-_pga);
                    }
                    return;
                case AgentState.Escape:
                    // 到达目的地，停止运动 距离目的地小于0.5f
                    if (translation.Value.x < -10f)
                    {
                        movementData.state = AgentState.Escaped;
                        movementData.escapeTime = elapsedTime;
                        translation.Value.y = -10;
                    }
                    return;
                case AgentState.Escaped:
                    velocity.Linear = float3.zero;
                    return;
                default:
                    return;
            }
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}
