using Unity.Entities;
using Unity.Physics;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class GlobalGravitySystem : SystemBase
{
    static readonly float basicGravity = -9.81f;

    protected override void OnUpdate()
    {
        var vertialAcc = GetSingleton<AccTimerData>().acc.y;
        if (vertialAcc != 0)
        {
            var setting = GetSingleton<PhysicsStep>();
            setting.Gravity.y = basicGravity - vertialAcc;
            SetSingleton<PhysicsStep>(setting);
        }

    }
}
