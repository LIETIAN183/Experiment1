using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;

// [UpdateAfter(typeof(InitializeSystem))]
public class AgentSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<AgentTag>().ForEach((ref PhysicsVelocity physicsVelocity, in AgentData agent) =>
        {
            physicsVelocity.Linear = agent.targetDirection * 6;
            // translation.Value += agentData.targetDirection * 7 * deltaTime;
        }).Schedule();

    }
}