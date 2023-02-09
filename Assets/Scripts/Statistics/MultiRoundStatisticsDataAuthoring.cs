using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

public enum AnalysisStage { DataBackup, Start, Simulation, Recover }

public struct MultiRoundStatisticsData : IComponentData
{
    // 多轮仿真的仿真状态标识
    public AnalysisStage curStage;
    // 当前要仿真地震事件的编号
    public int curSeismicIndex;
    // 地震事件总数
    public int seismicEventsCount;
    // PGA最大值，包含
    public float pgaThreshold; // 为 0 时按事件原 PGA 仿真
    // PGA从0到设定的最大值中多轮仿真每次增加的值
    public float pgaStep;// 不为 0 时按照间隔依次仿真，为 0 时只仿真 pgaThreshold 一次
    // 当前要仿真地震事件的目标 PGA
    public float curSimulationTargetPGA;
}

public class MultiRoundStatisticsDataAuthoring : MonoBehaviour { }

public class MultiRoundStatisticsDataAuthoringBaker : Baker<MultiRoundStatisticsDataAuthoring>
{
    public override void Bake(MultiRoundStatisticsDataAuthoring authoring)
    {
        AddComponent<MultiRoundStatisticsData>();
    }
}