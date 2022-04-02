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
        Entities.WithAll<ComsData>().ForEach((ref Translation translation, ref Rotation rotation, in ComsData data) =>
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
        }).ScheduleParallel();

        Entities.WithAll<SubShakeData>().ForEach((ref Translation translation, ref Rotation rotation, in SubShakeData data) =>
        {
            translation.Value = data.originLocalPosition;
            rotation.Value = data.originalRotation;
        }).ScheduleParallel();

        Entities.WithAll<AgentMovementData>().ForEach((ref Translation translation, ref AgentMovementData data, ref DynamicBuffer<TrajectoryBufferElement> trajectory) =>
        {
            translation.Value = data.originPosition;
            data.state = AgentState.NotActive;
            data.reactionTime = 0;
            data.escapeTime = 0;
            data.pathLength = 0;
            data.lastPosition = 0;
            trajectory.Clear();
        }).ScheduleParallel();

        ECSUIController.Instance.startBtn.GetComponent<CanvasGroup>().interactable = true;
        ECSUIController.Instance.EqSelector.GetComponent<CanvasGroup>().interactable = true;
        this.Enabled = false;
        simulation.GetExistingSystem<AnalysisSystem>().escapedBackup = 0;
    }
}


