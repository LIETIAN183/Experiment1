using Unity.Collections;
using Unity.Entities;
// using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;

// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class PathDisplaySystem : SystemBase
{
    // public LineRenderer line;
    // int count;
    // private BuildPhysicsWorld buildPhysicsWorld;

    protected override void OnCreate()
    {
        // this.Enabled = false;
        // line = new LineRenderer();

    }
    protected override void OnUpdate()
    {
        Entities.WithoutBurst().ForEach((in Translation trans, in AgentMovementData data) =>
        {
            LineDebug.instance.addPosition(trans.Value);
        }).Run();
    }
}