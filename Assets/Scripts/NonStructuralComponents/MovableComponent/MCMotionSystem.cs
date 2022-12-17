using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Mathematics;

// [AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public partial class MCMotionSystem : SystemBase
{
    static readonly float basicGravity = -9.81f;

    // 初始化成员变量
    protected override void OnCreate() => this.Enabled = false;

    protected override void OnStartRunning()
    {
        Entities.WithAll<MCData>().WithName("MCInitialize").ForEach((ref MCData curData) =>
        {
            curData.previousVelinY = 0;
        }).ScheduleParallel();
        // 初始化完成后才能开始下一步
        this.CompleteDependency();
    }
    protected override void OnUpdate()
    {

        // 获得时间间隔，地震加速度，当前垂直重力加速度
        var time = SystemAPI.Time.DeltaTime;
        //TODO: 测试返回值是否正确
        var accTimerData = GetSingleton<AccTimerData>();
        var seismicAcc = accTimerData.acc;
        var currentGravity = basicGravity - seismicAcc.y;
        seismicAcc *= accTimerData.envEnhanceFactor;

        Entities.WithAll<MCData>().WithName("MCHorizontalMotion").ForEach((ref PhysicsVelocity physicsVelocity, ref MCData data, in PhysicsMass physicsMass) =>
        {
            data.inAir = false;
            // 垂直加速度大于重力加速度的2/3即可认为在空中
            // physicsVelocity.Linear.y - data.previousVelinY / time <= currentGravity * 2 / 3
            if ((physicsVelocity.Linear.y - data.previousVelinY) * 3 <= currentGravity * 2 * time)
            {
                // 空气阻力 k = 1/2ρc_{d}A = 0.01f;ρ = 1.29;c_{d} = 0.8;A = 0.02
                physicsVelocity.ApplyLinearImpulse(physicsMass, -math.length(physicsVelocity.Linear) * 0.01f * physicsVelocity.Linear * time);
                data.inAir = true;
            }
            // 添加地震力
            physicsVelocity.ApplyLinearImpulse(physicsMass, -seismicAcc / physicsMass.InverseMass * time);

            data.previousVelinY = physicsVelocity.Linear.y;
        }).ScheduleParallel();
        // this.CompleteDependency();
    }
}
