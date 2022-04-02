using Unity.Entities;
using Unity.Mathematics;
[GenerateAuthoringComponent]
public struct AccTimerData : IComponentData
{
    public int gmIndex;

    // 数据的时间间隔
    public float dataDeltaTime;
    // 计数，同时是读取数据的 Index
    public int timeCount;
    // 根据仿真时间判断 timeCount 的添加量
    public int increaseNumber;

    public float3 acc;

    public float3 groundVel;

    public float elapsedTime;

    public float pga;
}
