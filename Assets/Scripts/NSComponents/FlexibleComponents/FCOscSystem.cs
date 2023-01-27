using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Physics;

// [AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateAfter(typeof(TimerSystem))]
[BurstCompile]
public partial struct FCOscSystem : ISystem, ISystemStartStop
{
    private ComponentLookup<FCData> m_fcDataLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireAnyForUpdate(state.GetEntityQuery(ComponentType.ReadOnly<FCData>()), state.GetEntityQuery(ComponentType.ReadOnly<SubFCData>()));
        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        m_fcDataLookup = state.GetComponentLookup<FCData>(true);

        var job1 = new ResetFCData().ScheduleParallel(state.Dependency);

        var job2 = new ResetSubFCData().ScheduleParallel(state.Dependency);

        state.Dependency = JobHandle.CombineDependencies(job1, job2);
        // 初始化完成后才能开始下一步
        state.CompleteDependency();
    }
    [BurstCompile]
    public void OnStopRunning(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var timerData = SystemAPI.GetSingleton<TimerData>();
        var deltaTime = SystemAPI.Time.DeltaTime;

        var job1 = new CalFCMotion
        {
            deltaTime = deltaTime,
            seismicAcc = timerData.curAcc * timerData.envEnhanceFactor
        }.ScheduleParallel(state.Dependency);

        m_fcDataLookup.Update(ref state);
        var job2 = new CalSubFCMotion
        {
            fcDataLookup = m_fcDataLookup,
            deltaTime = deltaTime
        }.ScheduleParallel(job1);

        state.Dependency = JobHandle.CombineDependencies(job1, job2);
    }
}

[BurstCompile]
// [WithAll()]
partial struct ResetFCData : IJobEntity
{
    void Execute(ref FCData fcData)
    {
        fcData.topDis = fcData.topVel = 0;
    }
}
[BurstCompile]
partial struct ResetSubFCData : IJobEntity
{
    void Execute(ref SubFCData subFCData, in LocalTransform localTransform)
    {
        subFCData.orgRot = localTransform.Rotation;
        subFCData.orgPos = localTransform.Position;
    }
}
[BurstCompile]
partial struct CalFCMotion : IJobEntity
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float3 seismicAcc;
    void Execute(ref FCData fcData, in LocalToWorld ltd)
    {
        fcData.forward = ltd.Forward;
        // 单面货架撞墙
        if (fcData.directionConstrain && fcData.topDis < 0 && fcData.topVel < 0)
        {
            fcData.topVel *= -0.3f;
            fcData.topDis += fcData.topVel * deltaTime;
        }

        // 货架受力振荡
        var strength = math.dot(seismicAcc, fcData.forward);
        fcData.topAcc = strength;
        float2 y = new float2(fcData.topDis, fcData.topVel);
        float2 y_derivative = new float2(y.y, -(fcData.k * y.x + fcData.c * y.y) / fcData.mass - strength);
        var y_tPlus1 = y + deltaTime * y_derivative;
        var y_tPlus1_derivative = new float2(y_tPlus1.y, -(fcData.k * y_tPlus1.x + fcData.c * y_tPlus1.y) / fcData.mass - strength);
        var t_result = y + deltaTime * 0.5f * (y_derivative + y_tPlus1_derivative);
        (fcData.topDis, fcData.topVel) = (t_result.x, t_result.y);
    }
}

[BurstCompile]
[WithNone(typeof(MCData)), WithAll(typeof(SubFCData))]
partial struct CalSubFCMotion : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<FCData> fcDataLookup;
    [ReadOnly] public float deltaTime;
    void Execute(ref PhysicsVelocity velocity, in SubFCData subFCData, in LocalTransform localTransform, in PhysicsMass mass)
    {
        FCData parentData = fcDataLookup[subFCData.parent];
        // w(h) = Δx*h^2(3L-h)/2L^3

        // set k = Δx/2L^3
        var k = parentData.topDis / (2 * math.pow(parentData.length, 3));
        // h^2 name hSquare
        var hSquare = subFCData.height * subFCData.height;

        // var curmovement = math.pow(curData.height, 2) * (3 * parentData.length - curData.height) * parentData.endMovement / (2 * math.pow(parentData.length, 3));
        var curmovement = k * hSquare * (3 * parentData.length - subFCData.height);

        // w'(h)= Δx(6Lh-3h^2)/2L^3=tanθ
        // var gradient = -3 * parentData.endMovement * (math.pow(curData.height, 2) - 2 * parentData.length * curData.height) / (2 * math.pow(parentData.length, 3));
        var radius = math.atan(k * (6 * parentData.length * subFCData.height - 3 * hSquare));
        // Euler 使用的是Radius，不再是Nomo时的角度
        // https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/api/Unity.Mathematics.quaternion.html?q=quaternion#Unity_Mathematics_quaternion_Euler_System_Single_System_Single_System_Single_Unity_Mathematics_math_RotationOrder_

        RigidTransform rgTransform = new RigidTransform(math.mul(subFCData.orgRot, quaternion.Euler(radius, 0, 0)), subFCData.orgPos + parentData.forward * curmovement);

        velocity = PhysicsVelocity.CalculateVelocityToTarget(mass, localTransform.Position, localTransform.Rotation, rgTransform, 1 / deltaTime);
    }
}