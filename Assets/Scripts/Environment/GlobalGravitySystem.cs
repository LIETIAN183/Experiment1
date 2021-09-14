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
        var vertialAcc = GetSingleton<AccTimerData>().acc.y;

        Entities.ForEach((ref PhysicsStep setting) =>
        {
            // 减去是因为，对物体要施加反向的地震加速度，地面不动时，物体获得反向的地震惯性力
            setting.Gravity.y = basicGravity - vertialAcc;
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
