using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(GroundMotionSystem))]
[UpdateAfter(typeof(EqControllerSystem))]
public class ComsMotionSystem : SystemBase
{
    int gmIndex = 0;
    int timeCount = 0;

    protected override void OnCreate()
    {
        // base.OnCreate();
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        // var deltaTime = Time.DeltaTime;
        ref BlobArray<GroundMotion> gmArray = ref GroundMotionBlobAssetsConstructor.gmBlobRefs[gmIndex].Value.gmArray;
        float3 acc = gmArray[timeCount].acceleration;
        // PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        Entities
        .WithAll<ComsTag>()
        .ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation, ref PhysicsMass physicsMass) =>
        {
            // Not Work
            // int rigidBodyIndex = physicsWorld.GetRigidBodyIndex(e);
            // float3 centreOfMass = physicsWorld.GetCenterOfMass(rigidBodyIndex);
            // float3 centreOfMass = physicsMass.GetCenterOfMassWorldSpace(translation, rotation);
            // physicsWorld.ApplyImpulse(rigidBodyIndex, acc / physicsMass.InverseMass * 0.01f, centreOfMass);

            physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, acc / physicsMass.InverseMass * 0.01f, physicsMass.CenterOfMass);


        }).ScheduleParallel();
        timeCount++;
    }

    public void Active(int Index)
    {
        timeCount = 0;
        gmIndex = Index;
        this.Enabled = true;
    }
}
