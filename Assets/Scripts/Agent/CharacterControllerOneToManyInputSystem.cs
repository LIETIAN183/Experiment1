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
        float2 movement = float2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            movement += new float2(0, 1);
            // direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += new float2(0, -1);
            // direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += new float2(-1, 0);
            // direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += new float2(1, 0);
            // direction += Vector3.right;
        }
        Entities
            .WithName("CharacterControllerOneToManyInputSystemJob")
            .WithBurst()
            .ForEach((ref CharacterControllerInternalData ccData) =>
            {
                ccData.Input.Movement = movement;
                // ccData.Input.Looking = input.Looking;
            }
            ).ScheduleParallel();

        var x = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var time = Time.DeltaTime;
        Entities
        .WithAll<AgentTag>()
        .WithBurst()
        .ForEach((ref PhysicsVelocity velocity, ref PhysicsMass mass) =>
        {
            // ccData.Input.Movement = movement;
            velocity.Linear = x;
            // mass.InverseInertia = float3.zero;
            // ccData.Input.Looking = input.Looking;
        }
        ).ScheduleParallel();
    }
}
