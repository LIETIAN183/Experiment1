using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MCData : IComponentData
{
    // 用于判断空中状态
    public float previousVelinY;
    public bool inAir;
}