namespace CatanGame.Core.Models;

public class EdgePosition
{
    public int Q { get; set; }
    public int R { get; set; }
    public int Direction { get; set; }

    public EdgePosition(int q, int r, int direction)
    {
        Q = q;
        R = r;
        Direction = direction;
    }

    public override bool Equals(object? obj)
    {
        if (obj is EdgePosition other)
        {
            return Q == other.Q && R == other.R && Direction == other.Direction;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R, Direction);
    }
}
