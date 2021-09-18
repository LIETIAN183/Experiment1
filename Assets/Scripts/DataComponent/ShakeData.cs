using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[GenerateAuthoringComponent]
public struct ShakeData : IComponentData
{
    // TODO: 如果物体变化过大，初始旋转度数就会存在误差
    /// <summary>
    /// 初始旋转度数
    /// </summary>

    public float strength;

    public float length;

    public float endMovement;

    public float shakeFrequency;

    // Simlified Method Configurion
    public bool simplifiedMethod;

    public float pastRadius;

    public quaternion worldRotation;
}
