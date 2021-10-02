using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public class ComsShakeSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var acc = GetSingleton<AccTimerData>().acc;
        var time = Time.DeltaTime;

        Entities.WithAll<ShakeData>().WithName("ComsBend").ForEach((ref ShakeData data, ref Rotation rotation, in LocalToWorld ltd) =>
        {
            data.endMovement += data.velocity * time;
            data.velocity += data._acc * time;
            // 单边限制
            // if (data.directionConstrain && data.endMovement < 0 && data.velocity < 0)
            // {
            //     data.velocity *= -0.3f;
            // }
            // else
            // {
            //     data.velocity += data._acc * time;
            // }
            data.strength = math.dot(-acc, ltd.Forward) / math.dot(ltd.Forward, ltd.Forward);
            data._acc = -data.k * data.endMovement - data.c * data.velocity + data.strength;
        }).ScheduleParallel();
    }

    protected override void OnStopRunning()
    {
        Entities.WithAll<ShakeData>().ForEach((ref ShakeData data) =>
        {
            data.strength = 0;
            data.endMovement = 0;
            data.velocity = 0;
            data._acc = 0;
        }).ScheduleParallel();
    }
}
