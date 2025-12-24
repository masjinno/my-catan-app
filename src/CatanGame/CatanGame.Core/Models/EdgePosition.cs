namespace CatanGame.Core.Models;

/// <summary>
/// 六角形タイルの辺の位置を表すクラス
/// 六角形座標系（Axial Coordinates）と辺の方向を使用して辺を一意に識別する
/// </summary>
public class EdgePosition
{
    /// <summary>
    /// タイルのQ座標（六角形座標系の右下向き軸）
    /// 右下方向を正とする
    /// </summary>
    public int Q { get; set; }

    /// <summary>
    /// タイルのR座標（六角形座標系の真下軸）
    /// 真下方向を正とする
    /// </summary>
    public int R { get; set; }

    /// <summary>
    /// 辺の方向（0-5の整数）
    /// 右下の辺を0として時計回りに割り振られる
    /// 0=右下、1=下、2=左下、3=左上、4=上、5=右上
    /// </summary>
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
