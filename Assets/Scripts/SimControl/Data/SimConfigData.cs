using Unity.Entities;
public struct SimConfigData : IComponentData
{
    public bool simEnvironment;

    public bool itemDestructible;

    public bool simFlowField;

    public bool simAgent;

    public bool displayTrajectories;

    public bool performStatistics;

    public int simIter;

    public float average;
}