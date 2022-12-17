using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(250)]
public struct PosBuffer : IBufferElementData
{
    public float3 pos;

    public static implicit operator float3(PosBuffer posBufferElement) => posBufferElement.pos;

    public static implicit operator PosBuffer(float3 p) => new PosBuffer { pos = p };
}

public class PosBufferAuthoring : MonoBehaviour { }

public class PosBufferAuthoringBaker : Baker<PosBufferAuthoring>
{
    public override void Bake(PosBufferAuthoring authoring)
    {
        AddBuffer<PosBuffer>();
    }
}