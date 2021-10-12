using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class GlobalGravitySystem : SystemBase
{
    static readonly float basicGravity = -9.81f;

    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var setting = GetSingleton<PhysicsStep>();
        setting.Gravity.y = basicGravity - GetSingleton<AccTimerData>().acc.y;
        SetSingleton<PhysicsStep>(setting);
    }

    protected override void OnStopRunning()
    {
        var setting = GetSingleton<PhysicsStep>();
        setting.Gravity.y = basicGravity;
        SetSingleton<PhysicsStep>(setting);
    }
}
