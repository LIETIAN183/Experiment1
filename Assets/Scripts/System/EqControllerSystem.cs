using System.Security.AccessControl;
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

        // Control Gound
        Entities.WithAll<GroundTag>().WithName("GroundMove").ForEach((ref PhysicsVelocity physicsVelocity) =>
        {
            // DeltaTime 为 0.01f ,因为已经设置了 DeltaTime 为固定值，那就不用每次再获取 DeltaTime了
            physicsVelocity.Linear += acc * 0.01f;
        }).ScheduleParallel();

        float3 verticalAcc = new float3(0, acc.y, 0);
        acc.y = 0;
        float havokCoefficeitn = 0.05f;
        // Control Coms
        Entities
        .WithAll<ComsTag>()
        .WithName("ComsMove")
        .ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation, ref PhysicsMass physicsMass) =>
        {
            // 可能施加力的方向是 Local 的导致无故弹跳
            physicsVelocity.ApplyImpulse(physicsMass, translation, rotation, acc / physicsMass.InverseMass * 0.01f * havokCoefficeitn, physicsMass.CenterOfMass);
            physicsVelocity.ApplyLinearImpulse(physicsMass, verticalAcc / physicsMass.InverseMass * 0.01f);
        }).ScheduleParallel();

        // 货架是弹性物体，会发生晃动，有两种方法，一种直接操作 Mesh ，但是性能消耗过大， 第二种是拼接多个 Cube， 操作每一个 Cube的位置变动，使用 Bezier 曲线[https://www.jianshu.com/p/332afb0fd10c]或者 Cantilever beams(悬臂梁)弯曲算法[https://www.zhihu.com/question/62427957/answer/198908295]
        // 但是均性能消耗过大，最终选择直接通过操作 Rotation 完成进行效果
        // TODO: 使用 Cantilever beams 算法实现弯曲
        // TODO: 判断货架站立状态
        float rotationAngle = 1;
        float rotationDirection = 0;
        if (acc.z > 0)
        {
            rotationDirection = 1;
        }
        else
        {
            rotationDirection = -1;
        }
        // 若是使用 Bend.BaseRotation,则 power = 0.005f,但是这样物体的旋转会受到影响
        // TODO: 使用 rotation.Value 目前调试值为 。00067f, 效果不佳
        float power = .005f;
        // TODO: 归一化strength
        float strength = math.sqrt(acc.x * acc.x + acc.z * acc.z);

        Entities.WithAll<BendTag>().WithName("Bend").ForEach((ref Rotation rotation, in BendTag bend, in LocalToWorld localToWorld) =>
        {
            // -：自身坐标系和世界坐标系夹角大于90，需要旋转反向
            // rotationDirectio: 旋转方向和地震方向相关
            // rotationAngle: 基础旋转角度
            // power: 基础强度系数
            // strength * math.abs(localToWorld.Forward.z): 地震强度系数乘以与物体可摇晃方向的夹角
            float desireRotationAngle = rotationDirection * rotationAngle * power * strength * math.abs(localToWorld.Forward.z);

            // localToWorld.forwold 值小于 0 表示自身坐标系和世界坐标系的夹角大于 90 度，更相近于旋转 180 度后的世界坐标系
            // 对于反向的物体，虽然旋转也是绕着 X 轴旋转，但是需要旋转的角度相反
            if (localToWorld.Forward.z < 0)
            {
                rotation.Value = math.mul(bend.baseRotation, quaternion.RotateX(-desireRotationAngle));
            }
            else
            {
                rotation.Value = math.mul(bend.baseRotation, quaternion.RotateX(desireRotationAngle));
            }
            // rotation.Value = math.mul(rotation.Value, quaternion.RotateY(1 * deltaTime));
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