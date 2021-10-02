using Unity.Entities;

[GenerateAuthoringComponent]
public struct ShakeData : IComponentData
{
    public float strength;

    public float length;

    public float endMovement;

    public float velocity;

    public float _acc;

    public float k, c;

    // true 时限制 SingleShelf 单向晃动
    public bool directionConstrain;
}
