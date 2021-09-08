using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class EnvInitialSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<SubShakeData>().ForEach((ref SubShakeData subBendData, in Rotation rotation, in Translation translation) =>
        {
            subBendData.originLocalPosition = translation.Value;
        }).ScheduleParallel();
        Enabled = false;
    }
}
