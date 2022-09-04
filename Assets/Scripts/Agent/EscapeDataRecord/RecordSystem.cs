using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class RecordSystem : SystemBase
{
    protected override void OnCreate() => this.Enabled = false;

    protected override void OnStartRunning()
    {
        Entities.WithAll<Escaping>().ForEach((ref RecordData recordData, in Translation translation) =>
        {
            recordData.lastposition = translation.Value;
            recordData.escapeTime = 0;
            recordData.escapeLength = 0;
            recordData.escapeAveVel = 0;
            recordData.accumulatedY = 0;
        }).ScheduleParallel(Dependency).Complete();
    }
    protected override void OnUpdate()
    {
        Entities.WithAll<Escaping>().ForEach((ref RecordData recordData, in Translation translation) =>
        {
            recordData.escapeLength += math.length(translation.Value.xz - recordData.lastposition.xz);
            recordData.accumulatedY += math.abs(translation.Value.y - recordData.lastposition.y);
            recordData.lastposition = translation.Value;
        }).ScheduleParallel();

        var elapsedTime = GetSingleton<AccTimerData>().elapsedTime;
        Entities.WithAll<Escaped>().WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled).ForEach((ref RecordData recordData) =>
        {
            if (recordData.escapeTime.Equals(0))
            {
                recordData.escapeTime = elapsedTime;
                recordData.escapeAveVel = recordData.escapeLength / elapsedTime;
            }
        }).ScheduleParallel();
    }
}
