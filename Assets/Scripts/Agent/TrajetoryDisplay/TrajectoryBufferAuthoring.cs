using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TrajectoryBufferAuthoring : MonoBehaviour
{

    class Baker : Baker<TrajectoryBufferAuthoring>
    {
        public override void Bake(TrajectoryBufferAuthoring authoring)
        {
            AddBuffer<TrajectoryBuffer>();
        }
    }
}

[InternalBufferCapacity(1000)]
public struct TrajectoryBuffer : IBufferElementData
{
    public float3 position;

    public static implicit operator float3(TrajectoryBuffer trajectoryBufferElement) => trajectoryBufferElement.position;

    public static implicit operator TrajectoryBuffer(float3 e) => new TrajectoryBuffer { position = e };
}

