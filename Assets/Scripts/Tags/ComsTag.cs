using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ComsTag : IComponentData
{
    // 用于判断空中状态
    public float previous_y;

    // 以下参数仅用于数据统计分析
    // 用于判断掉落状态和最终位移
    public float3 originPosition;

    // 用于区分组别
    public int groupID;
}