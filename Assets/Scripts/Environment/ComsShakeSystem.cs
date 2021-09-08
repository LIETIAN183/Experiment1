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
        var timerData = GetSingleton<AccTimerData>();
        var acc = timerData.acc;
        var elapsedTime = timerData.timeCount * 0.01f;

        Entities.WithAll<ShakeData>().WithName("ComsBend").ForEach((ref ShakeData data, ref Rotation rotation, in LocalToWorld ltd) =>
        {
            data.strength = math.abs(math.dot(acc, ltd.Forward) / math.dot(ltd.Forward, ltd.Forward));

            data.endMovement = 0.05f * data.strength * math.cos((2 * math.PI / data.shakePeriod) * elapsedTime + 90);

            if (data.simplifiedMethod)
            {
                var gradient = -3 * data.endMovement * (math.pow(0.37f, 2) - 2 * data.length * 0.37f) / (2 * math.pow(data.length, 3));
                var radius = math.atan(gradient);
                rotation.Value = math.mul(rotation.Value, quaternion.RotateX(radius - data.pastRadius));
                data.pastRadius = radius;
            }
        }).ScheduleParallel();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        Entities.WithAll<ShakeData>().ForEach((ref ShakeData data) =>
        {
            data.strength = 0;
            data.endMovement = 0;
        }).ScheduleParallel();
    }
}
