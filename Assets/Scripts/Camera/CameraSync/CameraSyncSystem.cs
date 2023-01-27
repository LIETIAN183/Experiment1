using Unity.Entities;
using UnityEngine;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(TimerSystem))]
public partial class CameraSyncSystem : SystemBase
{
    public Transform CameraTransformInGO;
    private Vector3 vel;

    protected override void OnCreate() => this.Enabled = false;
    protected override void OnUpdate()
    {
        this.Enabled = false;
        var time = SystemAPI.Time.DeltaTime;
        Vector3 acc = SystemAPI.GetSingleton<TimerData>().curAcc;
        vel -= acc * time;
        CameraTransformInGO.position += vel * time * 2;
    }
}
