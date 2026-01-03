using CatanGame.Core.Models;
using System.Windows;

namespace CatanGame.App.ViewModels;

public class EdgeViewModel : ViewModelBase
{
    private const double HexSize = 52;
    public EdgePosition Position { get; }
    public Point StartPoint { get; }
    public Point EndPoint { get; }

    private bool _isClickable;
    public bool IsClickable
    {
        get => _isClickable;
        set => SetProperty(ref _isClickable, value);
    }

    private bool _hasRoad;
    public bool HasRoad
    {
        get => _hasRoad;
        set => SetProperty(ref _hasRoad, value);
    }

    private PlayerColor? _roadOwner;
    public PlayerColor? RoadOwner
    {
        get => _roadOwner;
        set
        {
            if (SetProperty(ref _roadOwner, value))
            {
                OnPropertyChanged(nameof(RoadColor));
            }
        }
    }

    public string RoadColor
    {
        get
        {
            return RoadOwner switch
            {
                PlayerColor.Red => "#FF0000",
                PlayerColor.Blue => "#0000FF",
                PlayerColor.White => "#FFFFFF",
                PlayerColor.Orange => "#FFA500",
                _ => "#808080"
            };
        }
    }

    public EdgeViewModel(EdgePosition position)
    {
        Position = position;
        var (start, end) = CalculateEdgePoints(position);
        StartPoint = start;
        EndPoint = end;
        IsClickable = false;
        HasRoad = false;
        RoadOwner = null;
    }

    private (Point start, Point end) CalculateEdgePoints(EdgePosition edge)
    {
        // タイルの中心座標を計算
        double tileX = HexSize * (3.0 / 2.0 * edge.Q);
        double tileY = HexSize * (Math.Sqrt(3) / 2.0 * edge.Q + Math.Sqrt(3) * edge.R);

        // 辺の両端の頂点の角度を計算
        // Direction 0の辺は、頂点0と頂点1を結ぶ
        double angle1InDegrees = 60 * edge.Direction;
        double angle2InDegrees = 60 * (edge.Direction + 1);

        double angle1InRadians = angle1InDegrees * Math.PI / 180.0;
        double angle2InRadians = angle2InDegrees * Math.PI / 180.0;

        // 辺の両端の座標を計算
        double startX = tileX + HexSize * Math.Cos(angle1InRadians);
        double startY = tileY + HexSize * Math.Sin(angle1InRadians);

        double endX = tileX + HexSize * Math.Cos(angle2InRadians);
        double endY = tileY + HexSize * Math.Sin(angle2InRadians);

        // Canvas全体の中心オフセット（400, 350）を追加
        return (new Point(startX + 400, startY + 350), new Point(endX + 400, endY + 350));
    }
}
