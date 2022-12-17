using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(1000)]
public struct TrajectoryBuffer : IBufferElementData
{
    public float3 position;

    public static implicit operator float3(TrajectoryBuffer trajectoryBufferElement) => trajectoryBufferElement.position;

    public static implicit operator TrajectoryBuffer(float3 e) => new TrajectoryBuffer { position = e };
}

public class TrajectoryBufferAuthoring : MonoBehaviour { }

public class TrajectoryBufferAuthoringBaker : Baker<TrajectoryBufferAuthoring>
{
    public override void Bake(TrajectoryBufferAuthoring authoring)
    {
        AddBuffer<TrajectoryBuffer>();
    }
}
