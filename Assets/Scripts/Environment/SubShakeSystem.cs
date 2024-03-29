using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ComsShakeSystem))]
public class SubShakeSystem : SystemBase
{
    static readonly float3 forward = new float3(0, 0, 1);
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        Entities.WithAll<SubShakeData>().WithName("SubBend").ForEach((ref SubShakeData curData, ref Translation translation, ref Rotation rotation, in Parent parent) =>
        {
            ShakeData parentData = GetComponentDataFromEntity<ShakeData>(true)[parent.Value];
            // w(h) = Δx*h^2(3L-h)/2L^3

            // set k = Δx/2L^3
            var k = parentData.endMovement / (2 * math.pow(parentData.length, 3));
            // h^2 name hSquare
            var hSquare = curData.height * curData.height;

            // var curmovement = math.pow(curData.height, 2) * (3 * parentData.length - curData.height) * parentData.endMovement / (2 * math.pow(parentData.length, 3));
            var curmovement = k * hSquare * (3 * parentData.length - curData.height);
            translation.Value = curData.originLocalPosition + forward * curmovement;

            // w'(h)= Δx(6Lh-3h^2)/2L^3=tanθ
            // var gradient = -3 * parentData.endMovement * (math.pow(curData.height, 2) - 2 * parentData.length * curData.height) / (2 * math.pow(parentData.length, 3));
            var gradient = k * (6 * parentData.length * curData.height - 3 * hSquare);
            var radius = math.atan(gradient);
            // Euler 使用的是Radius，不再是Nomo时的角度
            // https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/api/Unity.Mathematics.quaternion.html?q=quaternion#Unity_Mathematics_quaternion_Euler_System_Single_System_Single_System_Single_Unity_Mathematics_math_RotationOrder_
            rotation.Value = quaternion.Euler(radius, 0, 0);
        }).ScheduleParallel();

        // For SubShake without parent
        Entities.WithAll<SubShakeData>().WithName("SubBendWithoutParent").ForEach((ref SubShakeData curData, ref Translation translation, ref Rotation rotation) =>
        {
            ShakeData parentData = GetComponentDataFromEntity<ShakeData>(true)[curData.parent];
            var k = parentData.endMovement / (2 * math.pow(parentData.length, 3));
            var hSquare = curData.height * curData.height;
            var curmovement = k * hSquare * (3 * parentData.length - curData.height);
            // LocalToWorld ltd = GetComponentDataFromEntity<LocalToWorld>(true)[curData.parent];
            // 这里因为物体添加 PhysicsBody,子物体独立，所以 forward 变成世界方向，进而导致演示的时候两个物体摇晃方向不出错
            translation.Value = curData.originLocalPosition + forward * curmovement;
            // translation.Value = curData.originLocalPosition + ltd.Forward * curmovement;
            var gradient = k * (6 * parentData.length * curData.height - 3 * hSquare);
            var radius = math.atan(gradient);
            rotation.Value = quaternion.Euler(radius, 0, 0);
        }).ScheduleParallel();
    }
}
