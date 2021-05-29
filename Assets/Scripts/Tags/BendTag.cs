using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BendTag : IComponentData
{
    // TODO: 如果物体变化过大，初始旋转度数就会存在误差
    /// <summary>
    /// 初始旋转度数
    /// </summary>
    public quaternion baseRotation;
}