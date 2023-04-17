using Unity.Mathematics;
using Unity.Burst;
using Unity.Physics;

[BurstCompile]
public static class Constants
{
    public static readonly float gravity = 9.81f;

    // Flow Field
    public static readonly int2 notInGridSet = new int2(-1, -1);
    public static readonly float3 halfHumanSize3D = new float3(0.25f, 0.85f, 0.25f);
    public static readonly float T_c = 500;
    public static readonly float T_i = 65535;
    public static readonly float c2_fluid = 4;
    public static readonly float c_s = 1;
    public static readonly float w_a = 10;
    public static readonly float w_avoid = 0.5f;
    public static readonly float c_local = 0.75f;
    public static readonly float destinationAgentOverlapRadius = 5f;

    // 计算行人指导方向时使用的位置偏移
    public static readonly float pedDirOffset = 0.2f;

    public static readonly CollisionFilter agentWallOnlyFilter = new CollisionFilter
    {
        BelongsTo = ~0u,
        CollidesWith = 101u << 29,
        GroupIndex = 0
    };

    public static readonly CollisionFilter agentOnlyFilter = new CollisionFilter
    {
        BelongsTo = ~0u,
        CollidesWith = 1u << 31,
        GroupIndex = 0
    };

    public static readonly CollisionFilter WallOnlyFilter = new CollisionFilter
    {
        BelongsTo = ~0u,
        CollidesWith = 1u << 29,
        GroupIndex = 0
    };

    public static readonly CollisionFilter ignorAgentGroundFilter = new CollisionFilter
    {
        BelongsTo = ~0u,
        CollidesWith = ~0u >> 2,
        GroupIndex = 0
    };

    public static readonly int[] dirIterOrder = new int[13] { 0, -10, 10, -20, 20, -30, 30, -40, 40, -50, 50, -60, 60 };

}