using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

[UpdateAfter(typeof(InitializeSystem))]
public class AgentSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.WithAll<AgentData>().ForEach((ref PhysicsVelocity physicsVelocity, in AgentData agent) =>
        {
            physicsVelocity.Linear = agent.targetDirection * 6;
            // translation.Value += agentData.targetDirection * 7 * deltaTime;
        }).Schedule();

    }
}