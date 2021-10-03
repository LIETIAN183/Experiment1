using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class ReloadSystem : SystemBase
{
    World simulation;
    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        simulation.GetExistingSystem<AccTimerSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = false;
        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = false;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = false;

        Entities.WithAll<ComsTag>().ForEach((ref Translation translation, ref Rotation rotation, in ComsTag data) =>
        {
            translation.Value = data.originPosition;
            rotation.Value = data.originRotation;
        }).ScheduleParallel();

        Entities.WithAll<ShakeData>().ForEach((ref ShakeData data) =>
        {
            data.strength = 0;
            data.endMovement = 0;
            data.velocity = 0;
            data._acc = 0;
            data.k = 160;
            data.c = 2;
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


