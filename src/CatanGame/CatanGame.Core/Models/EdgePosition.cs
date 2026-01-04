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

    /// <summary>
    /// 物理的に同じ辺を指す2つの表現のうち、正規化された1つの表現を返す
    /// 正規化ルール: 2つの(Q,R,Direction)のうち、辞書順で最小のものを返す
    /// </summary>
    public EdgePosition GetNormalized()
    {
        // 辺に隣接する2つのタイルの(Q, R, Direction)を列挙
        var candidates = new List<(int Q, int R, int Dir)>
        {
            (Q, R, Direction),
        };

        // 辺Directionに応じて、反対側のタイルの座標を追加
        switch (Direction)
        {
            case 0: // 右下の辺
                candidates.Add((Q + 1, R    , 3)); // 右下のタイルの左上の辺
                break;
            case 1: // 下の辺
                candidates.Add((Q    , R + 1, 4)); // 下のタイルの上の辺
                break;
            case 2: // 左下の辺
                candidates.Add((Q - 1, R + 1, 5)); // 左下のタイルの右上の辺
                break;
            case 3: // 左上の辺
                candidates.Add((Q - 1, R    , 0)); // 左上のタイルの右下の辺
                break;
            case 4: // 上の辺
                candidates.Add((Q    , R - 1, 1)); // 上のタイルの下の辺
                break;
            case 5: // 右上の辺
                candidates.Add((Q + 1, R - 1, 2)); // 右上のタイルの左下の辺
                break;
        }

        // 辞書順で最小のものを選択
        var normalized = candidates.OrderBy(c => c.Q).ThenBy(c => c.R).ThenBy(c => c.Dir).First();
        return new EdgePosition(normalized.Q, normalized.R, normalized.Dir);
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
