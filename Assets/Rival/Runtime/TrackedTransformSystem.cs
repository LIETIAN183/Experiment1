using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Rival
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class TrackedTransformFixedSimulationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach((ref TrackedTransform trackedTransform, in Translation translation, in Rotation rotation) =>
                {
                    trackedTransform.PreviousFixedRateTransform = trackedTransform.CurrentFixedRateTransform;
                    trackedTransform.CurrentFixedRateTransform = new RigidTransform(rotation.Value, translation.Value);
                }).Schedule();
        }
    }
}