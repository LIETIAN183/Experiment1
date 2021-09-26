using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Animation;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class SyncSystem : SystemBase
{

    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        var sync = GetSingleton<SyncTag>();
        sync.acc = GetSingleton<AccTimerData>().acc;
        SetSingleton<SyncTag>(sync);
    }

    protected override void OnStopRunning()
    {
        var sync = GetSingleton<SyncTag>();
        sync.acc = 0;
        SetSingleton<SyncTag>(sync);
    }
}
