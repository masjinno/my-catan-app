namespace CatanGame.Core.Models;

/// <summary>
/// 港を表すモデルクラス
/// 港はボードの外周タイルの海に面した辺に配置され、プレイヤーが資源を交換できる場所
/// </summary>
public class Port
{
    /// <summary>
    /// 港が配置されているタイルのQ座標（六角形座標系の水平軸方向）
    /// </summary>
    public int Q { get; set; }

    /// <summary>
    /// 港が配置されているタイルのR座標（六角形座標系の斜め軸方向）
    /// </summary>
    public int R { get; set; }

    /// <summary>
    /// 港が配置されている辺の方向（0-5）
    /// 0=右上、1=右、2=右下、3=左下、4=左、5=左上
    /// </summary>
    public int Direction { get; set; }

    /// <summary>
    /// 港の種類（一般港または専門港の資源タイプ）
    /// </summary>
    public PortType PortType { get; set; }

    /// <summary>
    /// 港のコンストラクタ
    /// </summary>
    /// <param name="q">タイルのQ座標</param>
    /// <param name="r">タイルのR座標</param>
    /// <param name="direction">辺の方向（0-5）</param>
    /// <param name="portType">港の種類</param>
    public Port(int q, int r, int direction, PortType portType)
    {
        Q = q;
        R = r;
        Direction = direction;
        PortType = portType;
    }
}
