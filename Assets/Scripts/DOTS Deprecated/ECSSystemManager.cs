using Unity.Entities;

[DisableAutoCreation]
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
        // 启动分析系统
        simulation.GetExistingSystem<AnalysisSystem>().Enabled = true;

        // SyncSystem、SubShakeSystem 可以选择不启用
        simulation.GetExistingSystem<AccTimerSystem>().Active(index);
        simulation.GetExistingSystem<GlobalGravitySystem>().Enabled = true;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = true;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = true;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = true;
        simulation.GetExistingSystem<SyncSystem>().Enabled = true;


    }

    public void Dective()
    {
        simulation.GetExistingSystem<AccTimerSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = false;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = false;
        simulation.GetExistingSystem<GlobalGravitySystem>().Enabled = false;
        simulation.GetExistingSystem<SyncSystem>().Enabled = false;

        // simulation.GetExistingSystem<AnalysisSystem>().Deactive();
        ECSUIController.Instance.ShowNotification("Simulation End");
    }
}
