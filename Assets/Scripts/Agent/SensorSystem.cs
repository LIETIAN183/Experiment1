using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class SensorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<AgentTag>().ForEach((in AgentData agent) =>
         {

         }).Schedule();
    }
}
