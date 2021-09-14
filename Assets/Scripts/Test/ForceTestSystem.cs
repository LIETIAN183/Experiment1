using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Transforms;

// // 禁止自动生成
[DisableAutoCreation]
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class ForceTestSystem : SystemBase
{
    public static readonly float3 forward = new float3(0, 0, 1);

    protected override void OnUpdate()
    {
        var time = Time.DeltaTime;
        var acc = GetSingleton<AccTimerData>().acc;
        // 1m/s2 加速度
        Entities.WithAll<BlueTag>().ForEach((ref PhysicsVelocity vel, ref BlueTag blue, in Translation translation, in Rotation rotation, in PhysicsMass mass) =>
        {
            // vel.ApplyImpulse(mass, translation, rotation, forward * acc / mass.InverseMass * time, mass.CenterOfMass);
            vel.ApplyImpulse(mass, translation, rotation, -acc / mass.InverseMass * time, mass.CenterOfMass);
            blue.acc = acc;
        }).ScheduleParallel();

        Entities.WithAll<RedTag>().ForEach((ref PhysicsVelocity vel, in PhysicsMass mass) =>
        {
            vel.ApplyLinearImpulse(mass, -acc / mass.InverseMass * time);
        }).ScheduleParallel();
    }


}
