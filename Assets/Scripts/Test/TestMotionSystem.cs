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

// 禁止自动生成
[DisableAutoCreation]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(GroundMotionSystem))]
public class TestMotionSystem : SystemBase
{
    int gmIndex = 0;
    int timeCount = 0;

    protected override void OnCreate()
    {
        base.OnCreate();
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        ref BlobArray<GroundMotion> gmArray = ref GroundMotionBlobAssetsConstructor.gmBlobRefs[gmIndex].Value.gmArray;
        float3 acc = gmArray[timeCount].acceleration;
        PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        Entities.WithoutBurst().WithAll<TestTag>().ForEach((Entity e, ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation, ref PhysicsMass physicsMass) =>
        {
            // FIXME 近似相等， float 无法表示 0.01f，只能近似，故而无法避免的存在精度误差
            // FIXME 加速度数据可能需要 double 精度存储才能保证正确
            // int rigidBodyIndex = physicsWorld.GetRigidBodyIndex(e);
            // Debug.Log(rigidBodyIndex);
            // float3 centreOfMass = physicsWorld.GetCenterOfMass(rigidBodyIndex);
            // float3 centreOfMass = physicsMass.GetCenterOfMassWorldSpace(translation, rotation);
            // Debug.Log(0);
            // Debug.Log(centreOfMass);
            // Debug.Log(1);
            // Debug.Log(physicsWorld.GetRigidBodyIndex(e));
            // physicsVelocity.ApplyLinearImpulse(physicsMass, new float3(10, 0, 0) / physicsMass.InverseMass * 0.01f);
            physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, new float3(10, 0, 0) / physicsMass.InverseMass * 0.01f, physicsMass.CenterOfMass);
            // physicsWorld.ApplyImpulse(rigidBodyIndex, acc / physicsMass.InverseMass, physicsMass.CenterOfMass);
            // Debug.Log(physicsMass.CenterOfMass);
            // Debug.Log(physicsMass.InverseMass);
            // physicsVelocity.ApplyLinearImpulse(physicsMass, acc / physicsMass.InverseMass * deltaTime);


        }).Run();//ScheduleParallel();
        // Debug.Log("Excuted");
        timeCount++;
    }

    public void Active(int Index)
    {
        timeCount = 0;
        gmIndex = Index;
        this.Enabled = true;
    }
}
