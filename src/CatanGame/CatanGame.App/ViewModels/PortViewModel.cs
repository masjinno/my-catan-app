using System.Windows;
using System.Windows.Media;
using CatanGame.Core.Models;

namespace CatanGame.App.ViewModels;

/// <summary>
/// 港の表示ロジックを担当するViewModel
/// 港ラベルの位置、色、テキストを提供する
/// </summary>
public class PortViewModel : ViewModelBase
{
    private readonly Port _port;
    private const double HexSize = 52;

    /// <summary>
    /// PortViewModelのコンストラクタ
    /// </summary>
    /// <param name="port">表示する港のモデル</param>
    public PortViewModel(Port port)
    {
        _port = port;
    }

    /// <summary>
    /// 港ラベルに表示するテキスト
    /// 一般港は「3:1」、専門港は「資源名2:1」の形式
    /// </summary>
    public string DisplayText
    {
        get
        {
            return _port.PortType switch
            {
                PortType.Generic => "3：1",
                PortType.Wood => "木2：1",
                PortType.Brick => "土2：1",
                PortType.Sheep => "羊2：1",
                PortType.Wheat => "麦2：1",
                PortType.Ore => "鉄2：1",
                _ => ""
            };
        }
    }

    /// <summary>
    /// 港ラベルの回転角度（辺に平行になるように）
    /// </summary>
    public double Rotation
    {
        get
        {
            // 辺の方向に応じて回転角度を計算
            // Direction 0-5: 0=右上、1=右、2=右下、3=左下、4=左、5=左上
            // 辺の法線方向から90度引いて辺に平行にする
            return _port.Direction * 60 + 30 - 90;
        }
    }

    /// <summary>
    /// 港ラベルの背景色
    /// 港の種類に応じて異なる色を返す
    /// </summary>
    public Brush BackgroundColor
    {
        get
        {
            return _port.PortType switch
            {
                PortType.Generic => new SolidColorBrush(Color.FromRgb(180, 180, 180)), // 灰色（少し濃く）
                PortType.Wood => new SolidColorBrush(Color.FromRgb(34, 139, 34)),      // 深緑
                PortType.Brick => new SolidColorBrush(Color.FromRgb(178, 34, 34)),     // 深紅
                PortType.Sheep => new SolidColorBrush(Color.FromRgb(120, 200, 120)),   // 明緑（少し濃く）
                PortType.Wheat => new SolidColorBrush(Color.FromRgb(225, 180, 50)),    // ゴールデンロッド（少し明るく）
                PortType.Ore => new SolidColorBrush(Color.FromRgb(105, 105, 105)),     // 濃灰色
                _ => Brushes.White
            };
        }
    }

    /// <summary>
    /// 港ラベルの表示位置（Canvas上の座標）
    /// </summary>
    public Point Position
    {
        get
        {
            return CalculatePortPosition(_port.Q, _port.R, _port.Direction, HexSize);
        }
    }

    /// <summary>
    /// 港ラベルの表示位置を計算する
    /// 六角形座標系（q, r）と辺の方向から、Canvas上の実際の座標を計算
    /// </summary>
    /// <param name="q">タイルのQ座標</param>
    /// <param name="r">タイルのR座標</param>
    /// <param name="direction">辺の方向（0-5）</param>
    /// <param name="size">六角形のサイズ（HexSize）</param>
    /// <returns>Canvas上の座標</returns>
    private Point CalculatePortPosition(int q, int r, int direction, double size)
    {
        // 1. 六角形タイルの中心座標を計算（Canvas座標系の原点は左上）
        //    pointy-top六角形の配置公式を使用
        double x = size * 1.5 * q;
        double y = size * Math.Sqrt(3) * (r + q / 2.0);

        // 2. 辺の方向に応じて港ラベルの位置を決定
        //    辺の中点方向の角度を計算
        //    Direction 0-5: 0=右上、1=右、2=右下、3=左下、4=左、5=左上
        double angleDeg = direction * 60 + 30;
        double angleRad = angleDeg * Math.PI / 180.0;

        // 3. タイルの中心から辺の外側への距離を計算
        double hexToEdge = size * Math.Sqrt(3) / 2.0;  // apothem（中心から辺までの距離）
        double gap = 14.0;                              // 辺からラベル中心までの隙間
        double offsetDistance = hexToEdge + gap;

        // 4. 極座標から直交座標への変換
        double portX = x + offsetDistance * Math.Cos(angleRad);
        double portY = y + offsetDistance * Math.Sin(angleRad);

        // 5. ボードの中心をCanvasの中央に配置するためのオフセットを適用
        double canvasCenterX = 400; // Canvasの幅の半分
        double canvasCenterY = 350; // Canvasの高さの半分

        // 6. ラベルの左上座標を計算（ラベルのサイズは50x24）
        double labelWidth = 50.0;
        double labelHeight = 24.0;
        double finalX = portX + canvasCenterX - labelWidth / 2.0;
        double finalY = portY + canvasCenterY - labelHeight / 2.0;

        return new Point(finalX, finalY);
    }
}
