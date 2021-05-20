using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerControllerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Assign values to local variables captured in your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     float deltaTime = Time.DeltaTime;

        // This declares a new kind of job, which is a unit of work to do.
        // The job is declared as an Entities.ForEach with the target components as parameters,
        // meaning it will process all entities in the world that have both
        // Translation and Rotation components. Change it to process the component
        // types you want.
        float movementX = Input.GetAxis("Horizontal");
        float movementZ = Input.GetAxis("Vertical");
        // bool Shift = Input.GetKey(KeyCode.LeftShift);
        // float jump = Input.GetAxis("jump");

        // Entities.WithAll<PlayerTag>().ForEach((ref CharacterControllerData controller) =>
        // {
        //     // Implement the work to perform for each entity here.
        //     // You should only access data that is local or that is a
        //     // field on this job. Note that the 'rotation' parameter is
        //     // marked as 'in', which means it cannot be modified,
        //     // but allows this job to run in parallel with other jobs
        //     // that want to read Rotation component data.
        //     // For example,
        //     //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
        //     // Vector3 forward = new Vector3(camera.Forward.x, 0.0f, camera.Forward.z).normalized;
        //     // Vector3 right = new Vector3(camera.Right.x, 0.0f, camera.Right.z).normalized;
        //     float3 direction = new float3(movementX, 0.0f, movementZ);

        //     if (!MathUtils.IsZero(movementX) || !MathUtils.IsZero(movementZ))
        //     {
        //         controller.currentDirection = direction;
        //         controller.currentMagnitude = Shift ? 1.5f : 1.0f;
        //     }
        //     else
        //     {
        //         controller.currentMagnitude = 0.0f;
        //     }

        //     controller.jump = jump > 0.0f;

        // }).ScheduleParallel();
        Entities.WithAll<PlayerTag>().ForEach((ref CharacterControllerData ccdata) =>
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as 'in', which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
            // Vector3 forward = new Vector3(camera.Forward.x, 0.0f, camera.Forward.z).normalized;
            // Vector3 right = new Vector3(camera.Right.x, 0.0f, camera.Right.z).normalized;
            float3 direction = new float3(movementX, 0, movementZ);
            ccdata.currentDirection = math.normalize(direction);
            ccdata.currentMagnitude = 1.0f;
            if (direction.Equals(float3.zero))
            {
                ccdata.currentMagnitude = 0;
            }

            ccdata.jump = false;

        }).ScheduleParallel();
    }
}
