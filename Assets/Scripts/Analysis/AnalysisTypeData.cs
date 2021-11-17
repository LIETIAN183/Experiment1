using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

public enum AnalysisTasks { Idle, Start, Reload }

[GenerateAuthoringComponent]
public struct AnalysisTypeData : IComponentData
{
    public AnalysisTasks task;

    public int index;

    public int eqCount;

    public int cofficient;
}
