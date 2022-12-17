using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SimulationLayerConfigurationData : IComponentData
{
    public bool isSimulateEnvironment;

    public bool isItemBreakable;

    public bool isSimulateFlowField;

    public bool isSimulateAgent;

    public bool isDisplayTrajectories;

    public bool isPerformStatistics;
}

public class SimulationLayerConfigurationDataAuthoring : MonoBehaviour
{
    public bool isSimulateEnvironment;

    public bool isItemBreakable;

    public bool isSimulateFlowField;

    public bool isSimulateAgent;

    public bool isDisplayTrajectories;

    public bool isPerformStatistics;
}

public class SimulationLayerConfigurationDataAuthoringBaker : Baker<SimulationLayerConfigurationDataAuthoring>
{
    public override void Bake(SimulationLayerConfigurationDataAuthoring authoring)
    {
        AddComponent(new SimulationLayerConfigurationData
        {
            isSimulateEnvironment = authoring.isSimulateEnvironment,
            isItemBreakable = authoring.isItemBreakable,
            isSimulateFlowField = authoring.isSimulateFlowField,
            isSimulateAgent = authoring.isSimulateAgent,
            isDisplayTrajectories = authoring.isDisplayTrajectories,
            isPerformStatistics = authoring.isPerformStatistics
        });
    }
}
