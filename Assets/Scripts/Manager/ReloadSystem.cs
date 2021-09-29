using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class ReloadSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        Entities.WithAll<ComsTag>().ForEach((ref Translation translation, ref Rotation rotation, in ComsTag data) =>
        {
            translation.Value = data.originPosition;
            rotation.Value = data.originRotation;
        }).ScheduleParallel();

        Entities.WithAll<SubShakeData>().ForEach((ref Translation translation, ref Rotation rotation, in SubShakeData data) =>
        {
            translation.Value = data.originLocalPosition;
            rotation.Value = quaternion.Euler(0, 0, 0);
        }).ScheduleParallel();

        ECSUIController.Instance.startBtn.GetComponent<CanvasGroup>().interactable = true;
        ECSUIController.Instance.EqSelector.GetComponent<CanvasGroup>().interactable = true;
        ECSUIController.Instance.pauseBtn.GetComponent<CanvasGroup>().interactable = false;
        this.Enabled = false;
    }
}


