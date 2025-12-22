using CatanGame.Core.Models;
using System.Windows;

namespace CatanGame.App.ViewModels;

public class HexTileViewModel : ViewModelBase
{
    private readonly HexTile _tile;

    public HexTile Tile => _tile;

    public Point Center { get; }

    public string DisplayNumber => _tile.NumberToken?.ToString() ?? "";

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
        // Flat-top orientation (辺が接する配置)
        double x = size * (Math.Sqrt(3) * q + Math.Sqrt(3) / 2.0 * r);
        double y = size * (3.0 / 2.0 * r);
        return new Point(x, y);
    }
}
