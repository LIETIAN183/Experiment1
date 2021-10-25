using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public class AgentInitSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {

        Entities.ForEach((ref AgentMovementData data) =>
        {
            data.state = AgentState.Delay;

        }).ScheduleParallel();

        this.Enabled = false;
    }
}
