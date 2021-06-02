using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;

public class AgnetInitialSystem : SystemBase
{
    protected override void OnUpdate()
    {
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
