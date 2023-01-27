using Unity.Entities;

public struct FFVisTypeStateData : IComponentData
{
    public FlowFieldVisulizeType ffVisType;
}

public enum FlowFieldVisulizeType
{
    None,
    Grid,
    CostField,
    CostHeatMap,
    IntegrationField,
    IntegrationHeatMap,
    FlowField,
    DebugField
};