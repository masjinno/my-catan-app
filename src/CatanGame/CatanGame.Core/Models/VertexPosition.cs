namespace CatanGame.Core.Models;

public class VertexPosition
{
    public int Q { get; set; }
    public int R { get; set; }
    public int Direction { get; set; }

    public VertexPosition(int q, int r, int direction)
    {
        Q = q;
        R = r;
        Direction = direction;
    }

    /// <summary>
    /// 物理的に同じ頂点を指す3つの表現のうち、正規化された1つの表現を返す
    /// 正規化ルール: 3つの(Q,R,Direction)のうち、辞書順で最小のものを返す
    /// </summary>
    public VertexPosition GetNormalized()
    {
        // 頂点に隣接する3つのタイルの(Q, R, Direction)を列挙
        var candidates = new List<(int Q, int R, int Dir)>
        {
            (Q, R, Direction),
        };

        // 頂点Directionに応じて、隣接する他の2つのタイルの座標を追加
        switch (Direction)
        {
            case 0: // 右の頂点
                candidates.Add((Q + 1, R - 1, 2)); // 右上のタイル
                candidates.Add((Q + 1, R    , 4)); // 右下のタイル
                break;
            case 1: // 右下の頂点
                candidates.Add((Q + 1, R    , 3)); // 右下のタイル
                candidates.Add((Q    , R + 1, 5)); // 下のタイル
                break;
            case 2: // 左下の頂点
                candidates.Add((Q    , R + 1, 4)); // 下のタイル
                candidates.Add((Q - 1, R + 1, 0)); // 左下のタイル
                break;
            case 3: // 左の頂点
                candidates.Add((Q - 1, R + 1, 5)); // 左下のタイル
                candidates.Add((Q - 1, R    , 1)); // 左上のタイル
                break;
            case 4: // 左上の頂点
                candidates.Add((Q - 1, R    , 0)); // 左上のタイル
                candidates.Add((Q    , R - 1, 2));     // 上のタイル
                break;
            case 5: // 上の頂点
                candidates.Add((Q    , R - 1, 1));     // 左上のタイル
                candidates.Add((Q + 1, R - 1, 3));     // 右上のタイル
                break;
        }

        // 辞書順で最小のものを選択
        var normalized = candidates.OrderBy(c => c.Q).ThenBy(c => c.R).ThenBy(c => c.Dir).First();
        return new VertexPosition(normalized.Q, normalized.R, normalized.Dir);
    }

    public override bool Equals(object? obj)
    {
        if (obj is VertexPosition other)
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
