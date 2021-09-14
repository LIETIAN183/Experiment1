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
        var acc = GetSingleton<AccTimerData>().acc;
        Entities.ForEach((ref SyncTag sync) =>
        {
            sync.acc = acc;
        }).Schedule();
    }

    // 测试修改 gravity 参数是否影响全局重力加速度，经测试有效
    // protected override void OnStopRunning()
    // {
    //     Entities.ForEach((ref PhysicsStep setting) =>
    //     {
    //         setting.Gravity.y = 10;
    //     }).Schedule();
    //     base.OnStopRunning();
    // }
}
