using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

// This input system simply applies the same character input
// information to every character controller in the scene
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(DemoInputGatheringSystem))]
public class CharacterControllerOneToManyInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Read user input
        var input = GetSingleton<CharacterControllerInput>();
        if (Input.GetKey(KeyCode.W))
        {
            input.Movement += new float2(0, 1);
            // direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            input.Movement += new float2(0, -1);
            // direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            input.Movement += new float2(-1, 0);
            // direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            input.Movement += new float2(1, 0);
            // direction += Vector3.right;
        }
        Entities
            .WithName("CharacterControllerOneToManyInputSystemJob")
            .WithBurst()
            .ForEach((ref CharacterControllerInternalData ccData) =>
            {
                ccData.Input.Movement = input.Movement;
                ccData.Input.Looking = input.Looking;
                // jump request may not be processed on this frame, so record it rather than matching input state
                if (input.Jumped != 0)
                    ccData.Input.Jumped = 1;
            }
            ).ScheduleParallel();
    }
}
