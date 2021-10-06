using Unity.Entities;

// [DisableAutoCreation]
[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
[UpdateAfter(typeof(CalculateFlowFieldSystem))]
public class DebugSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        GridDebug.instance.ClearList();
        Entities.ForEach((Entity entity, in CellData cellData) =>
        {
            GridDebug.instance.AddToList(cellData);
        }).Run();
    }
}
