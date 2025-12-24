namespace CatanGame.Core.Models;

/// <summary>
/// 六角形タイルを表すクラス
/// カタンのゲームボードを構成する基本要素
/// </summary>
public class HexTile
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
    /// タイルが産出する資源の種類
    /// </summary>
    public ResourceType ResourceType { get; set; }

    /// <summary>
    /// タイルに配置された数字チップ（2-12の範囲、砂漠タイルの場合はnull）
    /// </summary>
    public int? NumberToken { get; set; }

    /// <summary>
    /// 盗賊がこのタイルに配置されているかどうか
    /// </summary>
    public bool HasRobber { get; set; }

    public HexTile(int q, int r, ResourceType resourceType, int? numberToken = null)
    {
        Q = q;
        R = r;
        ResourceType = resourceType;
        NumberToken = numberToken;
        HasRobber = resourceType == ResourceType.Desert;
    }
}
