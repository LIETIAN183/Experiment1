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
        // Shakeillustration 场景不激活 GroundMotionSystem 和 ComsMotionSystem，激活 SubShakeSystem
        // 为了体现地震的效果，可以激活 ConsMotionSystem，但不作用于货架
        // simulation.GetExistingSystem<GroundMotionSystem>().Enabled = true;
        // simulation.GetExistingSystem<ComsMotionSystem>().Enabled = true;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = true;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = true;

    }

    public void Dective()
    {
        simulation.GetExistingSystem<AccTimerSystem>().Enabled = false;
        simulation.GetExistingSystem<GroundMotionSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = false;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = false;
        ECSUIController.Instance.ShowNotification("Simulation End");
    }
}
