using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

// This input system simply applies the same character input
// information to every character controller in the scene
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [UpdateAfter(typeof(DemoInputGatheringSystem))]
public class CharacterControllerOneToManyInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Read user input
        // var input = GetSingleton<CharacterControllerInput>();
        // float2 movement = float2.zero;
        // if (Input.GetKey(KeyCode.W))
        // {
        //     movement += new float2(0, 1);
        //     // direction += Vector3.forward;
        // }
        // if (Input.GetKey(KeyCode.S))
        // {
        //     movement += new float2(0, -1);
        //     // direction += Vector3.back;
        // }
        // if (Input.GetKey(KeyCode.A))
        // {
        //     movement += new float2(-1, 0);
        //     // direction += Vector3.left;
        // }
        // if (Input.GetKey(KeyCode.D))
        // {
        //     movement += new float2(1, 0);
        //     // direction += Vector3.right;
        // }
        // Entities
        //     .WithName("CharacterControllerOneToManyInputSystemJob")
        //     .WithBurst()
        //     .ForEach((ref CharacterControllerInternalData ccData) =>
        //     {
        //         ccData.Input.Movement = movement;
        //         // ccData.Input.Looking = input.Looking;
        //     }
        //     ).ScheduleParallel();
        DynamicBuffer<CellBufferElement> buffer = GetBufferFromEntity<CellBufferElement>(true)[GetSingletonEntity<FlowFieldSettingData>()];
        DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();
        var settingData = GetSingleton<FlowFieldSettingData>();

        float deltaTime = Time.DeltaTime;

        if (cellBuffer.Length == 0) return;

        Entities.WithReadOnly(cellBuffer).ForEach((ref CharacterControllerInternalData data, in Translation translation) =>
        {
            int2 localCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(settingData.originPoint, translation.Value, settingData.gridSize, settingData.cellRadius * 2);

            int flatLocalCellIndex = FlowFieldHelper.ToFlatIndex(localCellIndex, settingData.gridSize.y);
            float2 moveDirection = cellBuffer[flatLocalCellIndex].bestDirection;
            var vel = moveDirection * 2 * deltaTime;
            data.Input.Movement += vel * deltaTime;
        }).ScheduleParallel();
    }
}
