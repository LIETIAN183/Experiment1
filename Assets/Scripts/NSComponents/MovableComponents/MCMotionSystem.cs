using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateAfter(typeof(TimerSystem))]
[BurstCompile]
public partial struct MCMotionSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MCData>();
        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        // 仿真开始前重置 MCData, 且重置完毕后再 Update
        new ResetMCData().ScheduleParallel(state.Dependency).Complete();
    }
    [BurstCompile]
    public void OnStopRunning(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var timerData = SystemAPI.GetSingleton<TimerData>();
        var mcJob = new UpdateMCMotion
        {
            // 设置时间间隔，地震加速度，当前垂直重力加速度
            deltaTime = SystemAPI.Time.DeltaTime,
            seismicAcc = timerData.curAcc * timerData.envEnhanceFactor,
            currentGravity = Constants.gravity - timerData.curAcc.y
        }.ScheduleParallel(state.Dependency);
        state.Dependency = mcJob;
    }
}

[BurstCompile]
partial struct ResetMCData : IJobEntity
{
    void Execute(ref MCData mcData)
    {
        mcData.preVelinY = 0;
        mcData.inAir = false;
    }
}
[BurstCompile]
[WithNone(typeof(FCData)), WithAll(typeof(MCData))]
partial struct UpdateMCMotion : IJobEntity
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float3 seismicAcc;
    [ReadOnly] public float currentGravity;
    void Execute(ref MCData mcData, ref PhysicsVelocity velocity, in PhysicsMass mass)
    {
        mcData.inAir = false;
        // 垂直加速度大于重力加速度的2/3即可认为在空中
        // physicsVelocity.Linear.y - data.previousVelinY / time <= currentGravity * 2 / 3
        if ((velocity.Linear.y - mcData.preVelinY) * 1.5f <= currentGravity * deltaTime)
        {
            // 空气阻力 k = 1/2ρc_{d}A = 0.01f;ρ = 1.29;c_{d} = 0.8;A = 0.02
            velocity.ApplyLinearImpulse(mass, (-math.length(velocity.Linear) * 0.01f * deltaTime) * velocity.Linear);
            mcData.inAir = true;
        }
        // 添加地震力
        velocity.ApplyLinearImpulse(mass, -seismicAcc * (deltaTime / mass.InverseMass));

        mcData.preVelinY = velocity.Linear.y;
    }
}