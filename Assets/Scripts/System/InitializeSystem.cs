using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

// Execute Once
public class InitializeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref BendTag bend, in Rotation rotation) =>
        {
            bend.baseRotation = rotation.Value;
        }).ScheduleParallel();
        UnityEngine.Debug.Log(nameof(InitializeSystem) + " run.");
        Enabled = false;
    }
}
