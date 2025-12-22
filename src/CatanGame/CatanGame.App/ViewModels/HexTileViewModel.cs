using CatanGame.Core.Models;
using System.Windows;

namespace CatanGame.App.ViewModels;

public class HexTileViewModel : ViewModelBase
{
    private readonly HexTile _tile;

    public HexTile Tile => _tile;

    public Point Center { get; }

    public string DisplayNumber => _tile.NumberToken?.ToString() ?? "";

    public string NumberColor => _tile.NumberToken switch
    {
        6 => "#FF0000",
        8 => "#FF0000",
        _ => "#000000"
    };

    public string ResourceColor => _tile.ResourceType switch
    {
        ResourceType.Wood => "#228B22",
        ResourceType.Brick => "#8B4513",
        ResourceType.Sheep => "#90EE90",
        ResourceType.Wheat => "#FFD700",
        ResourceType.Ore => "#808080",
        ResourceType.Desert => "#F4A460",
        _ => "#FFFFFF"
    };

    public HexTileViewModel(HexTile tile, double hexSize)
    {
        _tile = tile;
        Center = CalculateHexCenter(tile.Q, tile.R, hexSize);
    }

    private Point CalculateHexCenter(int q, int r, double size)
    {
        // Pointy-top orientation の配置計算
        // タイル自体は30度回転させているが、配置計算はpointy-topのまま
        // 水平方向の幅 = size * 1.5
        // 垂直方向の高さ = size * √3

        double x = size * 1.5 * q;
        double y = size * Math.Sqrt(3) * (r + q / 2.0);

        // ボードの中心をCanvasの中央に配置するためのオフセット
        // HexagonControlのサイズ（120x104）の半分を引いて、中心座標を左上座標に変換
        double offsetX = 400 - 60; // Canvasの幅800の半分 - HexagonControlの幅の半分
        double offsetY = 350 - 52; // Canvasの高さ700の半分 - HexagonControlの高さの半分

        return new Point(x + offsetX, y + offsetY);
    }
}
