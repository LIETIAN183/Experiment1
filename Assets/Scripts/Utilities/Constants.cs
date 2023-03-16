using Unity.Mathematics;
using Unity.Burst;
using Unity.Physics;

[BurstCompile]
public static class Constants
{
    public static readonly float gravity = 9.81f;

    public static readonly int2 notInGridSet = new int2(-1, -1);

    public static readonly float3 halfHumanSize3D = new float3(0.25f, 0.85f, 0.25f);

    // Threshold_cost
    public static readonly float T_c = 500;

    public static readonly float T_i = 65535;

    public static readonly CollisionFilter agentOnlyFilter = new CollisionFilter
    {
        BelongsTo = ~0u,
        CollidesWith = 1u << 31,
        GroupIndex = 0
    };

    public static readonly CollisionFilter ignorAgentGroundFilter = new CollisionFilter
    {
        BelongsTo = ~0u,
        CollidesWith = ~0u >> 2,
        GroupIndex = 0
    };

    public static readonly float c2_fluid = 4;
    public static readonly float c3 = 10;
    public static readonly float w_a = 10;
    public static readonly float c_avoid = 0.5f;

    public static readonly float destinationAgentOverlapRadius = 5f;

}