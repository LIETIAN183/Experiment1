using Unity.Entities;
using UnityEngine;


[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class CameraSyncSystem : SystemBase
{
    public Transform CameraTransformInGO;
    private Vector3 vel;
    private float coefficient;

    protected override void OnCreate()
    {
        this.RequireForUpdate<CameraRefData>();
        coefficient = 3;
        this.Enabled = false;
    }
    protected override void OnStartRunning()
    {
        CameraTransformInGO = SystemAPI.ManagedAPI.GetSingleton<CameraRefData>().mainCamera.transform;
    }
    protected override void OnUpdate()
    {
        var time = SystemAPI.Time.DeltaTime;
        Vector3 acc = SystemAPI.GetSingleton<TimerData>().curAcc;
        vel -= acc * time;
        CameraTransformInGO.position += vel * time * coefficient;
    }
}
