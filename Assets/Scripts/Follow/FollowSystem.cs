using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AccTimerSystem))]
[UpdateAfter(typeof(SubShakeSystem))]
public class FollowSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Random x = new Random();
        x.InitState();
        Entities.ForEach((ref TargetTag target, in Translation translation) =>
        {
            target.deltaMove = translation.Value - target.previousPosition;
            target.previousPosition = translation.Value;
        }).Run();

        var deltaMove = GetSingleton<TargetTag>().deltaMove;
        deltaMove.y = 0;
        Entities.WithAll<FollowTag>().ForEach((ref Translation translation) =>
        {
            translation.Value += deltaMove * x.NextFloat(0.2f, 1);
        }).Run();
    }
}
