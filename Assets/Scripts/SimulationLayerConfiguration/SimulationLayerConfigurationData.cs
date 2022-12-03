using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
[GenerateAuthoringComponent]
public struct SimulationLayerConfigurationData : IComponentData
{
    public bool isSimulateEnvironment;

    public bool isItemBreakable;

    public bool isSimulateFlowField;

    public bool isSimulateAgent;

    public bool isDisplayTrajectories;

    public bool isPerformStatistics;
}
