using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

public struct GridDirection
{
    public readonly int2 Vector;
    public static readonly GridDirection None = new GridDirection(0, 0);
    public static readonly GridDirection North = new GridDirection(0, 1);
    public static readonly GridDirection South = new GridDirection(0, -1);
    public static readonly GridDirection East = new GridDirection(1, 0);
    public static readonly GridDirection West = new GridDirection(-1, 0);
    public static readonly GridDirection NorthEast = new GridDirection(1, 1);
    public static readonly GridDirection NorthWest = new GridDirection(-1, 1);
    public static readonly GridDirection SouthEast = new GridDirection(1, -1);
    public static readonly GridDirection SouthWest = new GridDirection(-1, -1);

    public static readonly GridDirection[] EightDirections = new GridDirection[8] {
        NorthWest,
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West
        // North,
        // East,
        // South,
        // West,
        // NorthEast,
        // SouthEast,
        // SouthWest,
        // NorthWest
    };
    public static readonly GridDirection[] FourDirections = new GridDirection[4]
    {
        North,
        East,
        South,
        West
    };

    private GridDirection(int x, int y) => Vector = new int2(x, y);

    public static implicit operator int2(GridDirection direction) => direction.Vector;
}
