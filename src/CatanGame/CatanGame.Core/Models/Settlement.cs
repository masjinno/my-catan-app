namespace CatanGame.Core.Models;

/// <summary>
/// 開拓地または都市を表すクラス
/// ボード上の頂点（3つのタイルの交点）に配置される
/// </summary>
public class Settlement
{
    /// <summary>
    /// 開拓地が配置されている頂点の位置
    /// 物理的には3つのタイルの交点だが、1つの(Q, R, Direction)で表現される
    /// </summary>
    public VertexPosition Position { get; set; }

    /// <summary>
    /// この開拓地を所有するプレイヤー
    /// </summary>
    public Player Owner { get; set; }

    /// <summary>
    /// 都市かどうか
    /// false: 開拓地（勝利点1）
    /// true: 都市（勝利点2）
    /// </summary>
    public bool IsCity { get; set; }

    public Settlement(VertexPosition position, Player owner, bool isCity = false)
    {
        Position = position;
        Owner = owner;
        IsCity = isCity;
    }

    /// <summary>
    /// この開拓地/都市が提供する勝利点を取得
    /// </summary>
    /// <returns>開拓地なら1、都市なら2</returns>
    public int GetVictoryPoints()
    {
        return IsCity ? 2 : 1;
    }
}
