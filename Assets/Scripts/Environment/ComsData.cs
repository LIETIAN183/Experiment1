using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ComsData : IComponentData
{
    // 用于判断空中状态
    public float previous_VelInY;

    // 以下参数仅用于数据统计分析
    // 用于判断掉落状态和最终位移
    public float3 originPosition;

    // 只用于 ShakeIllustration 场景下物品的重置
    public quaternion originRotation;
}