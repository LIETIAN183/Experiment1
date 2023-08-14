using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateAfter(typeof(TimerSystem))]
[BurstCompile]
public partial struct HelperSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {

    }
    [BurstCompile]
    public void OnStopRunning(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // if (Input.GetKey(KeyCode.O))
        // {
        //     foreach (var velocity in SystemAPI.Query<RefRW<PhysicsVelocity>>().WithAll<HighLightTag>())
        //     {
        //         velocity.ValueRW.Angular = new float3(-1, 0, 0) * SystemAPI.Time.DeltaTime;
        //     }
        // }
        // else if (Input.GetKey(KeyCode.P))
        // {
        //     foreach (var velocity in SystemAPI.Query<RefRW<PhysicsVelocity>>().WithAll<HighLightTag>())
        //     {
        //         velocity.ValueRW.Angular = float3.zero;
        //     }
        // }

        // 用于 Virtual Drill 模型的复现
        // var acc = SystemAPI.GetSingleton<TimerData>().curAcc;
        // foreach (var velocity in SystemAPI.Query<RefRW<PhysicsVelocity>>().WithAll<HighLightTag>())
        // {
        //     velocity.ValueRW.Linear += acc * SystemAPI.Time.DeltaTime;
        // }
    }
}