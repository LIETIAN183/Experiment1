using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class ComsShakeSystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;

    protected override void OnUpdate()
    {
        var acc = GetSingleton<AccTimerData>().acc;
        var time = Time.DeltaTime;
        Debug.Log(time);
        var multiple = time / 0.01f * 2f;

        Entities.WithAll<ShakeData>().WithName("ComsBend").ForEach((ref ShakeData data, ref Rotation rotation, in LocalToWorld ltd) =>
        {
            // 计算地震强度
            // data.strength = math.dot(acc, ltd.Forward) / math.dot(ltd.Forward, ltd.Forward);
            // // 计算终端位移
            // data.endMovement = 0.05f * math.abs(data.strength) * math.cos((2 * math.PI * data.shakeFrequency) * elapsedTime + 90);
            // data.endMovement = 0.2f;
            // x计算真实位移，endmovement需要调整后再输出
            data.endMovement += data.velocity * time;
            // 单边限制
            // Returns b if c is true, a otherwise.
            data.velocity = math.select(data.velocity + data._acc * time, data.velocity * -0.3f, data.directionConstrain && data.endMovement < 0 && data.velocity < 0);
            data.strength = math.dot(-acc * 2f, ltd.Forward);
            // 放大实验采样频率，c需要乘以相应倍数
            data._acc = -data.k * data.endMovement - data.c * multiple * data.velocity + data.strength;

            // 采用近似算法，整体旋转
            // if (data.simplifiedMethod)
            // {
            //     data.worldRotation = ltd.Rotation;
            //     var gradient = -3 * data.endMovement * (math.pow(0.37f, 2) - 2 * data.length * 0.37f) / (2 * math.pow(data.length, 3));
            //     var radius = math.atan(gradient);
            //     rotation.Value = math.mul(rotation.Value, quaternion.RotateX(radius - data.pastRadius));
            //     data.pastRadius = radius;
            // }
        }).ScheduleParallel();
    }
}
