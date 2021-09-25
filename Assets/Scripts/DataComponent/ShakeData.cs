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

    public float velocity;

    public float _acc;

    public float _x;

    public float k, c;

    // 用于独立子碰撞体跟随
    public float3 deltaMove;

    // 0 不限制运动， 1 限制正向晃动， 2 限制反向晃动
    public int constrainDirection;

    // Simlified Method Configurion
    // public bool simplifiedMethod;

    // public float pastRadius;

    // public quaternion worldRotation;
}
