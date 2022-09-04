using Unity.Entities;
using Unity.Jobs;

// 地震开始，开始逃生
[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
public partial class SeismicActiveSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_EndSimECBSystem;

    protected override void OnCreate()
    {
        m_EndSimECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        if (World.DefaultGameObjectInjectionWorld.GetExistingSystem<AccTimerSystem>().Enabled)
        {
            var ecb = m_EndSimECBSystem.CreateCommandBuffer().AsParallelWriter();

            var startHandle = Entities.WithAll<Idle>().ForEach((Entity e, int entityInQueryIndex) =>
            {
                ecb.RemoveComponent<Idle>(entityInQueryIndex, e);
                ecb.AddComponent<Escaping>(entityInQueryIndex, e);
            }).ScheduleParallel(Dependency);

            m_EndSimECBSystem.AddJobHandleForProducer(startHandle);

            Dependency = startHandle;
        }
    }
}
