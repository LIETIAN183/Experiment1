using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct TimerData : IComponentData
{
    // 存储当前地震事件的编号
    public int seismicEventIndex;
    // 存储当前地震事件名字
    public FixedString32Bytes seismicEventName;
    // 当前地震事件数据的记录时间间隔,目标物理仿真事件间隔, simDeltaTime 配置前需用 inRange(0.01f, 0.06f, 0.04f) 检查
    public float eventDeltaTime, simDeltaTime;
    // 根据事件间隔计算出每次仿真需要获得的地震数据的下标,根据 targetDeltaTime 判断 dataIndexInArray 的增长量
    public int accListIndex, accListIndexIncrement;
    // 当前时刻的地震加速度
    public float3 curAcc;
    // 已经逝去的时间，地震事件时长，额外延长的仿真时间
    public float elapsedTime, eventDuration;
    // 到当前时刻为止的最大地震加速度
    public float curPGA;
    // 事件的最大地震加速度，目标最大地震加速度，加速度调节量
    public float eventPGA, simPGA, adjustmentPGAFactor;

    // 调节低震级下的物品受影响程度
    public float envEnhanceFactor;
}