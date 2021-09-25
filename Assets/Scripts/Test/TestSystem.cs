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

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(GroundMotionSystem))]
[DisableAutoCreation]
public class TestSystem : SystemBase
{
    public float3 x;
    protected override void OnCreate()
    {
        x = new float3(0.76f, 0.55f, 4.2f);
    }
    protected override void OnUpdate()
    {
        var temp = x;
        Entities.WithAll<TestTag>().ForEach((ref Translation translation) =>
        {
            // temp += math.forward() * 3 * 0.01f;
            translation.Value += math.forward() * 3 * 0.01f;
        }).Run();
        x = temp;
    }

}
