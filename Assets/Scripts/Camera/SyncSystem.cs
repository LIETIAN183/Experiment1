using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class SyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sync = GetSingleton<SyncTag>();
        sync.acc = GetSingleton<AccTimerData>().acc;
        SetSingleton<SyncTag>(sync);
    }
}
