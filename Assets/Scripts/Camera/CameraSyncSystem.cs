using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class CameraSyncSystem : SystemBase
{
    public Transform CameraGameObjectTransform;

    private Vector3 vel;

    protected override void OnCreate()
    {
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        var time = Time.DeltaTime;
        Vector3 acc = GetSingleton<AccTimerData>().acc;
        vel -= acc * time;
        CameraGameObjectTransform.position += vel * time * 5;
    }
}
