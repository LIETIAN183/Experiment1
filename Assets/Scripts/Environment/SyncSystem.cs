using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class SyncSystem : SystemBase
{

    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var sync = GetSingleton<SyncTag>();
        sync.acc = GetSingleton<AccTimerData>().acc;
        SetSingleton<SyncTag>(sync);
        // Entities.ForEach((ref SyncTag sync) =>
        // {
        //     sync.acc = acc;
        // }).Schedule();
    }

    protected override void OnStopRunning()
    {
        var sync = GetSingleton<SyncTag>();
        sync.acc = 0;
        SetSingleton<SyncTag>(sync);
    }
}
