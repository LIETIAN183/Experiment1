using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[DisableAutoCreation]
public class AgentInit : SystemBase
{
    protected override void OnCreate()
    {
    }
    protected override void OnUpdate()
    {


        Entities.WithAll<AgentMovementData>().ForEach((ref PhysicsMass mass) =>
        {
            mass.InverseInertia = float3.zero;
        }).ScheduleParallel();

        this.Enabled = false;
    }
}
