using Unity.Entities;


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
