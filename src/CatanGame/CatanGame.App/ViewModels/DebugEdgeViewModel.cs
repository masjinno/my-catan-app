using System.Windows;
using System.Windows.Media;
using CatanGame.Core.Models;

namespace CatanGame.App.ViewModels;

/// <summary>
/// デバッグ用：辺の位置を可視化するViewModel
/// </summary>
public class DebugEdgeViewModel : ViewModelBase
{
    private const double HexSize = 52;

    public int Q { get; }
    public int R { get; }
    public int Direction { get; }
    public Brush Color { get; }

    public DebugEdgeViewModel(int q, int r, int direction, Brush color)
    {
        Q = q;
        R = r;
        Direction = direction;
        Color = color;
    }

    public Point Position
    {
        get
        {
            return CalculateEdgePosition(Q, R, Direction, HexSize);
        }
    }

    private Point CalculateEdgePosition(int q, int r, int direction, double size)
    {
        // 1. 六角形タイルの中心座標を計算
        double x = size * 1.5 * q;
        double y = size * Math.Sqrt(3) * (r + q / 2.0);

        // 2. 辺の中点方向の角度を計算
        double angleDeg = direction * 60 + 30;
        double angleRad = angleDeg * Math.PI / 180.0;

        // 3. タイルの中心から辺の中点までの距離
        double hexToEdge = size * Math.Sqrt(3) / 2.0;

        // 4. 極座標から直交座標への変換
        double edgeX = x + hexToEdge * Math.Cos(angleRad);
        double edgeY = y + hexToEdge * Math.Sin(angleRad);

        // 5. ボードの中心をCanvasの中央に配置
        double canvasCenterX = 400;
        double canvasCenterY = 350;

        // 6. 丸の左上座標を計算（丸のサイズは10x10）
        double circleSize = 10.0;
        double finalX = edgeX + canvasCenterX - circleSize / 2.0;
        double finalY = edgeY + canvasCenterY - circleSize / 2.0;

        return new Point(finalX, finalY);
    }
}
