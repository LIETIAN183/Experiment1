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
    GlobalFlowField,
    LocalFlowField,
    TargetField,
    DebugField1,
    DebugField2,
    DebugField3
};