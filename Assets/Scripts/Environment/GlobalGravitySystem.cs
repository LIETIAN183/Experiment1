using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class GlobalGravitySystem : SystemBase
{
    static readonly float basicGravity = -9.81f;

    protected override void OnUpdate()
    {
        // 根据地震输入获得垂直方向加速度修改量
        float gravityModify = GetSingleton<AccTimerData>().acc.y;
        if (gravityModify.Equals(0)) return;
        // 修改全局重力加速度
        PhysicsStep setting = GetSingleton<PhysicsStep>();
        setting.Gravity.y = basicGravity - gravityModify;
        SetSingleton<PhysicsStep>(setting);
    }
}
