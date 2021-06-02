using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerControllerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float movementX = Input.GetAxis("Horizontal");
        float movementZ = Input.GetAxis("Vertical");
        Entities.WithAll<AgentData>().ForEach((ref AgentData agentData, in Rotation rotation) =>
        {
            float3 direction = new float3(movementX, 0, movementZ);// TODO：写在 Foreach 外时可能获得 NULL 值，但不知道原因
            // var dir = math.normalize(math.mul(rotation.Value, direction));
            // physicsVelocity.Linear = direction * 8 * deltaTime;
            if (direction.Equals(float3.zero))
            {
                agentData.targetDirection = 0;
            }
            else
            {   // direction 为 0 时，归一化返回 NULL
                agentData.targetDirection = math.normalize(math.mul(rotation.Value, direction));
            }

            // float3 direction = new float3(movementX, 0, movementZ);
            // ccdata.currentDirection = math.normalize(math.mul(rotation.Value, direction));
            // ccdata.currentMagnitude = 1.0f;
            // if (direction.Equals(float3.zero))
            // {
            //     ccdata.currentMagnitude = 0;
            // }
        }).ScheduleParallel();
    }
}
