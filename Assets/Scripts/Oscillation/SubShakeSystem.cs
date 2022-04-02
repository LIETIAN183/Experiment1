using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Extensions;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ComsShakeSystem))]
public class SubShakeSystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;

    protected override void OnUpdate()
    {
        var time = Time.DeltaTime;
        // Entities.WithAll<SubShakeData>().WithName("SubBend").ForEach((ref SubShakeData curData, ref Translation translation, ref Rotation rotation, in Parent parent) =>
        // {
        //     ShakeData parentData = GetComponentDataFromEntity<ShakeData>(true)[parent.Value];
        //     // w(h) = Δx*h^2(3L-h)/2L^3

        //     // set k = Δx/2L^3
        //     var k = parentData.endMovement / (2 * math.pow(parentData.length, 3));
        //     // h^2 name hSquare
        //     var hSquare = curData.height * curData.height;

        //     // var curmovement = math.pow(curData.height, 2) * (3 * parentData.length - curData.height) * parentData.endMovement / (2 * math.pow(parentData.length, 3));
        //     var curmovement = k * hSquare * (3 * parentData.length - curData.height);
        //     // translation.Value += curData.originLocalPosition + math.forward() * curmovement - translation.Value;
        //     translation.Value = curData.originLocalPosition + math.forward() * curmovement;

        //     // w'(h)= Δx(6Lh-3h^2)/2L^3=tanθ
        //     // var gradient = -3 * parentData.endMovement * (math.pow(curData.height, 2) - 2 * parentData.length * curData.height) / (2 * math.pow(parentData.length, 3));
        //     var gradient = k * (6 * parentData.length * curData.height - 3 * hSquare);
        //     var radius = math.atan(gradient);
        //     // Euler 使用的是Radius，不再是Nomo时的角度
        //     // https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/api/Unity.Mathematics.quaternion.html?q=quaternion#Unity_Mathematics_quaternion_Euler_System_Single_System_Single_System_Single_Unity_Mathematics_math_RotationOrder_
        //     rotation.Value = quaternion.Euler(radius, 0, 0);
        // }).ScheduleParallel();

        Entities.WithAll<SubShakeData>().WithName("SubBend").ForEach((ref PhysicsVelocity velocity, ref Translation translation, ref Rotation rotation, in SubShakeData curData, in PhysicsMass mass) =>
        {
            ShakeData parentData = GetComponentDataFromEntity<ShakeData>(true)[curData.parent];
            // w(h) = Δx*h^2(3L-h)/2L^3

            // set k = Δx/2L^3
            var k = parentData.endMovement / (2 * math.pow(parentData.length, 3));
            // h^2 name hSquare
            var hSquare = curData.height * curData.height;

            // var curmovement = math.pow(curData.height, 2) * (3 * parentData.length - curData.height) * parentData.endMovement / (2 * math.pow(parentData.length, 3));
            var curmovement = k * hSquare * (3 * parentData.length - curData.height);

            // w'(h)= Δx(6Lh-3h^2)/2L^3=tanθ
            // var gradient = -3 * parentData.endMovement * (math.pow(curData.height, 2) - 2 * parentData.length * curData.height) / (2 * math.pow(parentData.length, 3));
            var radius = math.atan(k * (6 * parentData.length * curData.height - 3 * hSquare));
            // Euler 使用的是Radius，不再是Nomo时的角度
            // https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/api/Unity.Mathematics.quaternion.html?q=quaternion#Unity_Mathematics_quaternion_Euler_System_Single_System_Single_System_Single_Unity_Mathematics_math_RotationOrder_
            // 直接修改位置和旋转角度缺少物理计算
            // rotation.Value = math.mul(curData.originalRotation, quaternion.Euler(radius, 0, 0));
            // translation.Value = curData.originLocalPosition + parentData.forward * curmovement;

            RigidTransform rgtransform = new RigidTransform(math.mul(curData.originalRotation, quaternion.Euler(radius, 0, 0)), curData.originLocalPosition + parentData.forward * curmovement);

            velocity = PhysicsVelocity.CalculateVelocityToTarget(mass, translation, rotation, rgtransform, 1 / time);
        }).ScheduleParallel();
    }
}
