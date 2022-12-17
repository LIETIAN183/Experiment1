using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

public struct AccTimerData : IComponentData
{
    // 存储当前地震事件的编号
    public int seismicIndex;
    // 存储当前地震事件名字
    public FixedString32Bytes seismicName;
    // 当前地震事件数据的记录时间间隔,目标物理仿真事件间隔
    public float dataDeltaTime, simulationDeltaTime;
    // 根据事件间隔计算出每次仿真需要获得的地震数据的下标,根据 targetDeltaTime 判断 dataIndexInArray 的增长量
    public int accIndexInArray, increaseNumber;
    // 当前时刻的地震加速度
    public float3 acc;
    // 已经逝去的时间，地震结束的时间，额外延长的仿真时间
    public float elapsedTime, seismicFinishTime;
    // 到当前时刻为止的最大地震加速度
    public float curPGA;
    // 事件的最大地震加速度，目标最大地震加速度，加速度调节量
    public float eventPGA, targetPGA, magnitudeModification;

    // 调节低震级下的物品受影响程度
    public float envEnhanceFactor;
}

public class AccTimerDataAuthoring : MonoBehaviour { }

public class AccTimerDataAuthoringBaker : Baker<AccTimerDataAuthoring>
{
    public override void Bake(AccTimerDataAuthoring authoring)
    {
        AddComponent<AccTimerData>();
    }
}