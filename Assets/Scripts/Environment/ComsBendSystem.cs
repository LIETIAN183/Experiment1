using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class ComsBendSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var acc = GetSingleton<AccTimerData>().acc;
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

        Entities.WithAll<BendTag>().WithName("BendMotion").ForEach((ref Rotation rotation, in BendTag bend, in LocalToWorld localToWorld) =>
        {
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
        }).ScheduleParallel();
    }
}
