using Unity.Entities;
using UnityEngine;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public partial class CameraSyncSystem : SystemBase
{
    public Transform CameraTransformInGO;
    private Vector3 vel;

    protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        this.Enabled = false;
        var time = Time.DeltaTime;
        Vector3 acc = GetSingleton<AccTimerData>().acc;
        vel -= acc * time;
        CameraTransformInGO.position += vel * time * 2;
    }
}
