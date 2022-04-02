using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ShakeData : IComponentData
{
    public float strength;

    public float length;

    public float endMovement;

    public float velocity;

    public float _acc;

    public float k, c;

    public bool directionConstrain;

    public float3 forward;
}
