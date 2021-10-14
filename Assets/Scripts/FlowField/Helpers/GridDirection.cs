using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class GridDirection
{
    public readonly int2 Vector;

    private GridDirection(int x, int y)
    {
        Vector = new int2(x, y);
    }

    public static implicit operator int2(GridDirection direction)
    {
        return direction.Vector;
    }

    public static readonly GridDirection None = new GridDirection(0, 0);
    public static readonly GridDirection North = new GridDirection(0, 1);
    public static readonly GridDirection South = new GridDirection(0, -1);
    public static readonly GridDirection East = new GridDirection(1, 0);
    public static readonly GridDirection West = new GridDirection(-1, 0);
    public static readonly GridDirection NorthEast = new GridDirection(1, 1);
    public static readonly GridDirection NorthWest = new GridDirection(-1, 1);
    public static readonly GridDirection SouthEast = new GridDirection(1, -1);
    public static readonly GridDirection SouthWest = new GridDirection(-1, -1);

    public static readonly List<GridDirection> CardinalAndIntercardinalDirections = new List<GridDirection>
        {
            North,
            East,
            South,
            West,
            NorthEast,
            SouthEast,
            SouthWest,
            NorthWest
        };
}
