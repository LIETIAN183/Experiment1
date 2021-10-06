using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class CreateGridSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Get Setting Data
        FlowFieldSettingData settingComponent = GetSingleton<FlowFieldSettingData>();

#if UNITY_EDITOR
        // Connect SettingData to DisplayDebug
        GridDebug.instance.FlowFieldData = settingComponent;
#endif
        // Create Grid

        var _cellArchetype = EntityManager.CreateArchetype(typeof(CellData), typeof(GridCreatedTag));
        int2 gridSize = settingComponent.gridSize;
        EntityManager.CreateEntity(_cellArchetype, gridSize.x * gridSize.y);

        // SetupGridSystem Enabled
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<SetupGridSystem>().Enabled = true;

        // Disable CreateGridSystem
        this.Enabled = false;
    }
}
