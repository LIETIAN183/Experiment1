using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent, InternalBufferCapacity(1000)]
public struct TrajectoryBufferElement : IBufferElementData
{
    public float3 position;

    public static implicit operator float3(TrajectoryBufferElement trajectoryBufferElement)
    {
        return trajectoryBufferElement.position;
    }

    public static implicit operator TrajectoryBufferElement(float3 e)
    {
        return new TrajectoryBufferElement { position = e };
    }
}
