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

// 禁止自动生成
// [DisableAutoCreation]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class TestMoveSubEntitySystem : SystemBase
{
    protected override void OnCreate()
    {
        var x = new float3(0, 0, 1);
        var y = new float3(0, 0, -1);
        Debug.Log("dot:" + math.dot(x, y));
        Debug.Log("distance:" + math.distance(x, y));
        Debug.Log("distancesq:" + math.distancesq(x, y));
    }

    protected override void OnUpdate()
    {
        Entities.WithAll<TestTag>().ForEach((ref Translation translation) =>
        {
            translation.Value += new float3(1, 0, 0);
        }).ScheduleParallel();
    }


}
