using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FCData : IComponentData
{
    public float length;

    public float topDis, topVel, topAcc;

    public float k, c, mass;

    public bool directionConstrain;

    public float3 forward;
}
