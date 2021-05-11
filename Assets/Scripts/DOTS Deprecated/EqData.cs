using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

// Not used Now
[InternalBufferCapacity(6000)]
[System.Serializable]
public struct EqData : ISharedComponentData, IEquatable<EqData>
{
    // public List<Vector3> acc;
    // public NativeArray<float3> acc;
    public DynamicBuffer<float3> acc;
    public int timeLength;
    public int skipLine;
    public float gravityValue;
    public FixedString128 folder;


    public bool Equals(EqData other)
    {
        return acc.Equals(other.acc) &&
        timeLength == other.timeLength &&
        skipLine == other.skipLine &&
        gravityValue == other.gravityValue &&
        folder == other.folder;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        if (!ReferenceEquals(acc, null)) hash ^= acc.GetHashCode();
        hash ^= timeLength.GetHashCode();
        hash ^= skipLine.GetHashCode();
        hash ^= gravityValue.GetHashCode();
        hash ^= folder.GetHashCode();
        return hash;
    }
}
