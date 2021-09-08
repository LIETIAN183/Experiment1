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
            // w(x) = h^2(3L-h)*Δx/2L^3
            var curmovement = math.pow(curData.height, 2) * (3 * parentData.length - curData.height) * parentData.endMovement / (2 * math.pow(parentData.length, 3));
            translation.Value = curData.originLocalPosition + forward * curmovement;

            // w'(x)= Δx(6LX-3x^2)/2L^3=tanθ
            var gradient = -3 * parentData.endMovement * (math.pow(curData.height, 2) - 2 * parentData.length * curData.height) / (2 * math.pow(parentData.length, 3));
            var radius = math.atan(gradient);
            // Euler 使用的是Radius，不再是Nomo时的角度
            // https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/api/Unity.Mathematics.quaternion.html?q=quaternion#Unity_Mathematics_quaternion_Euler_System_Single_System_Single_System_Single_Unity_Mathematics_math_RotationOrder_
            rotation.Value = quaternion.Euler(radius, 0, 0);
        }).ScheduleParallel();

        // For SubShake without parent
        Entities.WithAll<SubShakeData>().WithName("SubBendWithoutParent").ForEach((ref SubShakeData curData, ref Translation translation, ref Rotation rotation) =>
        {
            ShakeData parentData = GetComponentDataFromEntity<ShakeData>(true)[curData.parent];
            var curmovement = math.pow(curData.height, 2) * (3 * parentData.length - curData.height) * parentData.endMovement / (2 * math.pow(parentData.length, 3));
            translation.Value = curData.originLocalPosition + forward * curmovement;
            var gradient = -3 * parentData.endMovement * (math.pow(curData.height, 2) - 2 * parentData.length * curData.height) / (2 * math.pow(parentData.length, 3));
            var radius = math.atan(gradient);
            rotation.Value = quaternion.Euler(radius, 0, 0);
        }).ScheduleParallel();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        Entities.WithAll<SubShakeData>().ForEach((ref Translation translation, ref Rotation rotation, in SubShakeData subBend) =>
        {
            translation.Value = subBend.originLocalPosition;
        }).ScheduleParallel();
    }
}
