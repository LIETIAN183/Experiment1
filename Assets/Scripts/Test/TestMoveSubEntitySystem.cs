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
[DisableAutoCreation]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class TestMoveSubEntitySystem : SystemBase
{

    protected override void OnUpdate()
    {
        Entities.WithAll<TestTag>().ForEach((ref Translation translation) =>
        {
            translation.Value += new float3(1, 0, 0);
        }).ScheduleParallel();
    }
}
