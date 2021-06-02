using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Physics;

[DisableAutoCreation]
// Execute Once
public class InitializeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Initial Bend Data
        Entities.WithAll<BendTag>().ForEach((ref BendTag bend, in Rotation rotation) =>
        {
            bend.baseRotation = rotation.Value;
        }).ScheduleParallel();
        // UnityEngine.Debug.Log(nameof(InitializeSystem) + " run.");

        // Set Agent Rotation Constraint
        Entities.WithAll<AgentData>().ForEach((ref PhysicsMass physicsMass) =>
        {
            // UnityPhysicsSamples 2b6.Motion Properties SetInertiaInverseBehaviour Script
            // mass.InverseInertia[0] = LockX ? 0 : mass.InverseInertia[0];
            // mass.InverseInertia[1] = LockY ? 0 : mass.InverseInertia[1];
            // mass.InverseInertia[2] = LockZ ? 0 : mass.InverseInertia[2];
            physicsMass.InverseInertia.xz = 0;
        }).ScheduleParallel();
        Enabled = false;
    }
}
