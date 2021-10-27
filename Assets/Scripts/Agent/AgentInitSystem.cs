using Unity.Entities;
using Unity.Jobs;

[DisableAutoCreation]
public class AgentInitSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithoutBurst().ForEach((ref AgentMovementData data) =>
        {
            data.state = AgentState.Delay;
            // TODO: 确定范围
            data.reactionTimeVariable = 0;
            // data.reactionTimeVariable = NormalDistribution.RandomGaussianInRange(0.7f, 1.3f);

        }).Run();
        this.Enabled = false;
    }
}
