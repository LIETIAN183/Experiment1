using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Stateful;

// 柔性构件振荡仿真
// [AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateAfter(typeof(TimerSystem))]
[BurstCompile]
public partial struct FCOscSystem : ISystem, ISystemStartStop
{
    private ComponentLookup<FCData> m_fcDataLookup;

    // private ComponentLookup<PhysicsVelocity> VelocityGroup;
    // private ComponentLookup<SubFCData> SubShakeGroup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FCData>();
        // state.RequireAnyForUpdate(state.GetEntityQuery(ComponentType.ReadOnly<FCData>()), state.GetEntityQuery(ComponentType.ReadOnly<SubFCData>()));
        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        // 柔性构件初始化数据
        m_fcDataLookup = state.GetComponentLookup<FCData>(true);
        // VelocityGroup = state.GetComponentLookup<PhysicsVelocity>(true);
        // SubShakeGroup = state.GetComponentLookup<SubFCData>(true);

        var job1 = new ResetFCDataJob().ScheduleParallel(state.Dependency);

        state.EntityManager.CompleteDependencyBeforeRO<LocalTransform>();
        var job2 = new ResetSubFCDataJob().ScheduleParallel(state.Dependency);

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

        // NativeArray<float3> FrictionBridge = new NativeArray<float3>(1, Allocator.TempJob);
        // FrictionBridge[0] = float3.zero;
        // bool isExceed = math.length(timerData.curAcc.xz) > 0.37f * 9.8f;
        // state.EntityManager.CompleteDependencyBeforeRO<PhysicsVelocity>();
        // VelocityGroup.Update(ref state);
        // SubShakeGroup.Update(ref state);

        // // 计算货架与其上物品之间的摩擦力
        // foreach (var elements in SystemAPI.Query<DynamicBuffer<StatefulCollisionEvent>>())
        // {
        //     foreach (var element in elements)
        //     {
        //         if (element.State == StatefulEventState.Stay)
        //         {
        //             var entityA = element.EntityA;
        //             var entityB = element.EntityB;

        //             bool isASubShake = SubShakeGroup.HasComponent(entityA);
        //             bool isBSubshake = SubShakeGroup.HasComponent(entityB);

        //             if (isASubShake && isBSubshake || !isASubShake && !isBSubshake)
        //             {
        //                 continue;
        //             }
        //             else
        //             {
        //                 var frictionValue = element.CollisionDetails.EstimatedImpulse;
        //                 var velDirection = math.select(VelocityGroup[entityA].Linear, VelocityGroup[entityB].Linear, isASubShake);
        //                 var normalDirection = math.select(element.GetNormalFrom(entityB), element.GetNormalFrom(entityA), isASubShake);
        //                 var velProjection = math.dot(velDirection, normalDirection) * math.normalizesafe(normalDirection);
        //                 var angle = UnityEngine.Vector3.Angle(velDirection, velProjection);
        //                 var frictionDirection = math.select(velDirection + velProjection, velDirection - velProjection, angle < 90);
        //                 FrictionBridge[0] += math.select(frictionDirection * frictionValue, -frictionDirection * frictionValue, isExceed);
        //             }
        //         }
        //     }
        // }
        // var friction = FrictionBridge[0];

        var job1 = new CalFCMotionJob
        {
            deltaTime = deltaTime,
            seismicAcc = timerData.curAcc * timerData.envEnhanceFactor,
            // friction = friction
        }.ScheduleParallel(state.Dependency);

        m_fcDataLookup.Update(ref state);
        var job2 = new CalSubFCMotionJob
        {
            fcDataLookup = m_fcDataLookup,
            deltaTime = deltaTime
        }.ScheduleParallel(job1);

        state.Dependency = JobHandle.CombineDependencies(job1, job2);

        // FrictionBridge.Dispose();
    }
}

// 重置货架振荡状态
[BurstCompile]
partial struct ResetFCDataJob : IJobEntity
{
    void Execute(ref FCData fcData)
    {
        fcData.topDis = fcData.topVel = 0;
    }
}

// 初始化变形子组件的初始空间数据
[BurstCompile]
partial struct ResetSubFCDataJob : IJobEntity
{
    void Execute(ref SubFCData subFCData, in LocalTransform localTransform)
    {
        subFCData.orgRot = localTransform.Rotation;
        subFCData.orgPos = localTransform.Position;
    }
}

// 计算柔性构件的振荡运动
[BurstCompile]
partial struct CalFCMotionJob : IJobEntity
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float3 seismicAcc;

    // [ReadOnly] public float3 friction;
    void Execute(ref FCData fcData, in LocalToWorld ltd)
    {
        fcData.forward = ltd.Forward;
        // 单面货架撞墙
        if (fcData.directionConstrain && fcData.topDis < 0 && fcData.topVel < 0)
        {
            fcData.topVel *= -0.3f;
            fcData.topDis += fcData.topVel * deltaTime;
        }

        // 考虑货架上物品的摩擦力
        // if (fcData.considerFriction)
        // {
        //     var strength = math.dot(seismicAcc, fcData.forward);
        //     fcData.topAcc = strength;
        //     float2 y = new float2(fcData.topDis, fcData.topVel);
        //     float2 y_derivative = new float2(y.y, -(fcData.k * y.x + fcData.c * y.y + math.dot(friction, ltd.Forward)) / fcData.mass - strength);
        //     var y_tPlus1 = y + deltaTime * y_derivative;
        //     var y_tPlus1_derivative = new float2(y_tPlus1.y, -(fcData.k * y_tPlus1.x + fcData.c * y_tPlus1.y + math.dot(friction, ltd.Forward)) / fcData.mass - strength);
        //     var t_result = y + deltaTime * 0.5f * (y_derivative + y_tPlus1_derivative);
        //     (fcData.topDis, fcData.topVel) = (t_result.x, t_result.y);
        // }
        // else
        // {
        // 货架受力振荡
        var strength = math.dot(seismicAcc, fcData.forward);
        fcData.topAcc = strength;
        float2 y = new float2(fcData.topDis, fcData.topVel);
        float2 y_derivative = new float2(y.y, -(fcData.k * y.x + fcData.c * y.y) / fcData.mass - strength);
        var y_tPlus1 = y + deltaTime * y_derivative;
        var y_tPlus1_derivative = new float2(y_tPlus1.y, -(fcData.k * y_tPlus1.x + fcData.c * y_tPlus1.y) / fcData.mass - strength);
        var t_result = y + deltaTime * 0.5f * (y_derivative + y_tPlus1_derivative);
        (fcData.topDis, fcData.topVel) = (t_result.x, t_result.y);
        // }
    }
}

// 实现柔性构件的变形仿真
[BurstCompile]
[WithNone(typeof(MCData)), WithAll(typeof(SubFCData))]
partial struct CalSubFCMotionJob : IJobEntity
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