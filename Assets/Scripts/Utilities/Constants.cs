using Unity.Mathematics;
using Unity.Burst;
[BurstCompile]
public static class Constants
{
    public static readonly float gravity = 9.81f;

    public static readonly int2 notInGridSet = new int2(-1, -1);

    public static readonly float3 halfHumanSize3D = new float3(0.25f, 0.85f, 0.25f);
}
