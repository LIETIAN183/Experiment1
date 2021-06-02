using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class EnvInitialSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<BendTag>().ForEach((ref BendTag bend, in Rotation rotation) =>
        {
            bend.baseRotation = rotation.Value;
        }).ScheduleParallel();
        Enabled = false;
    }
}
