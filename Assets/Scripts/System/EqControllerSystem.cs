using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [UpdateAfter(typeof(CollisionEventSystem))]
public class EqControllerSystem : SystemBase
{
    int gmIndex = 0;
    // 时间对应的加速度下标
    int timeCount = 0;
    protected override void OnCreate()
    {
        // base.OnCreate();
        var fixedSimulationGroup = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FixedStepSimulationSystemGroup>();
        fixedSimulationGroup.Timestep = 0.01f;
        this.Enabled = false;
        // 注册 ECS 单例模式
        // RequireSingletonForUpdate<EqMangerData>();
        // EntityManager.CreateEntity(typeof(EqMangerData));

    }

    protected override void OnUpdate()
    {

        float deltaTime = Time.DeltaTime;
        // Debug.Log(deltaTime);
        ref BlobArray<GroundMotion> gmArray = ref GroundMotionBlobAssetsConstructor.gmBlobRefs[gmIndex].Value.gmArray;
        float3 acc = gmArray[timeCount].acceleration;
        // ref BlobString name = ref gmAsset.gmName;
        // Debug.Log(name.ToString());
        // PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;


        // Control Gound
        Entities.WithAll<GroundTag>().WithName("GroundMove").ForEach((ref PhysicsVelocity physicsVelocity) =>
        {
            // accData.applyAcc = acc;
            // DeltaTime 为 0.01f ,因为已经设置了 DeltaTime 为固定值，那就不用每次再获取 DeltaTime了
            physicsVelocity.Linear += acc * 0.01f;
        }).ScheduleParallel();

        // acc *= 0.1f;
        acc.y = 0;
        float havokCoefficeitn = 0.1f;
        // Control Coms
        Entities
        .WithAll<ComsTag>()
        .WithName("ComsMove")
        .ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation, ref PhysicsMass physicsMass) =>//, in CollisionStateData collisionStateData
        {
            // Not Work
            // int rigidBodyIndex = physicsWorld.GetRigidBodyIndex(e);
            // float3 centreOfMass = physicsWorld.GetCenterOfMass(rigidBodyIndex);
            // float3 centreOfMass = physicsMass.GetCenterOfMassWorldSpace(translation, rotation);
            // physicsWorld.ApplyImpulse(rigidBodyIndex, acc / physicsMass.InverseMass * 0.01f, centreOfMass);
            // if (collisionStateData.isGround)
            // {
            // 可能施加力的方向是 Local 的导致无故弹跳
            physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, acc / physicsMass.InverseMass * 0.01f * havokCoefficeitn, physicsMass.CenterOfMass);
            // accData.applyAcc = float3.zero;
            // }

        }).ScheduleParallel();

        // Havok Physics 物体旋转，打击到其他小物体，致使弹飞
        // Entities
        // .WithAll<ComsTag>()
        // .ForEach((ref Translation translation, ref YaxisLimitData yaxisLimitData) =>//, in CollisionStateData collisionStateData
        // {
        //     if (translation.Value.y > yaxisLimitData.y_lateset)
        //     {
        //         translation.Value.y = yaxisLimitData.y_lateset;
        //     }
        //     if (translation.Value.y < 0.22f)
        //     {
        //         translation.Value.y = 0.22f;
        //     }
        //     yaxisLimitData.y_lateset = translation.Value.y;
        // }).ScheduleParallel();


        // Update UI
        ECSUIController.Instance.progress.currentValue = timeCount;

        // Update Time
        ++timeCount;
        if (timeCount >= gmArray.Length)
        {
            // Debug.Log("End");
            // TODO: UI 显示仿真结束提示
            this.Enabled = false;
        }
    }

    public void Active(int index)
    {
        timeCount = 0;
        gmIndex = index;
        this.Enabled = true;
    }

    public void Dective()
    {

    }
}
