using Unity.Entities;

public class ECSSystemManager : SystemBase
{
    World simulation;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;
    }

    protected override void OnUpdate()
    {
    }

    public void Active(int index)
    {
        // World.DefaultGameObjectInjectionWorld.GetExistingSystem<AccTimerSystem>().Active(index);
        simulation.GetExistingSystem<AccTimerSystem>().Active(index);
        simulation.GetExistingSystem<GroundMotionSystem>().Enabled = true;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = true;
        simulation.GetExistingSystem<ComsBendSystem>().Enabled = true;

    }

    public void Dective()
    {
        simulation.GetExistingSystem<AccTimerSystem>().Enabled = false;
        simulation.GetExistingSystem<GroundMotionSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsBendSystem>().Enabled = false;
    }
}
