using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// [AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
public partial class FCTopOscSystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;

    protected override void OnStartRunning()
    {
        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FCSubMotionSystem>().Enabled = true;
        Entities.WithAll<FCData>().WithName("FCInitialize").ForEach((ref FCData curData) =>
        {
            curData.topAcc = curData.topDis = curData.topVel = 0;
        }).ScheduleParallel();
        // 初始化完成后才能开始下一步
        this.CompleteDependency();
    }
    protected override void OnStopRunning()
    {
        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FCSubMotionSystem>().Enabled = false;
    }

    protected override void OnUpdate()
    {
        var accTimerData = GetSingleton<AccTimerData>();
        var acc = accTimerData.acc * accTimerData.envEnhanceFactor;
        var time = SystemAPI.Time.DeltaTime;

        Entities.WithAll<FCData>().WithName("FCTopOsc").ForEach((ref FCData data, in LocalToWorld ltd) =>
        {
            data.forward = ltd.Forward;
            // 单面货架撞墙
            if (data.directionConstrain && data.topDis < 0 && data.topVel < 0)
            {
                data.topVel *= -0.3f;
                data.topDis += data.topVel * time;
            }

            // 货架受力振荡
            var strength = math.dot(acc, data.forward);
            float2 y = new float2(data.topDis, data.topVel);
            float2 y_derivative = new float2(y.y, -(data.k * y.x + data.c * y.y) / data.mass - strength);
            var y_tPlus1 = y + time * y_derivative;
            var y_tPlus1_derivative = new float2(y_tPlus1.y, -(data.k * y_tPlus1.x + data.c * y_tPlus1.y) / data.mass - strength);
            var t_result = y + time * 0.5f * (y_derivative + y_tPlus1_derivative);
            (data.topDis, data.topVel) = (t_result.x, t_result.y);
        }).ScheduleParallel();
    }
}
