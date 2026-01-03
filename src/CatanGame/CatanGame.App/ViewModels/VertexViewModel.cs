using CatanGame.Core.Models;
using System.Windows;

namespace CatanGame.App.ViewModels;

public class VertexViewModel : ViewModelBase
{
    private const double HexSize = 52;
    public VertexPosition Position { get; }
    public Point Center { get; }

    private bool _isClickable;
    public bool IsClickable
    {
        get => _isClickable;
        set => SetProperty(ref _isClickable, value);
    }

    private bool _hasSettlement;
    public bool HasSettlement
    {
        get => _hasSettlement;
        set => SetProperty(ref _hasSettlement, value);
    }

    private PlayerColor? _settlementOwner;
    public PlayerColor? SettlementOwner
    {
        get => _settlementOwner;
        set
        {
            if (SetProperty(ref _settlementOwner, value))
            {
                OnPropertyChanged(nameof(SettlementColor));
            }
        }
    }

    private bool _isCity;
    public bool IsCity
    {
        get => _isCity;
        set => SetProperty(ref _isCity, value);
    }

    public string SettlementColor
    {
        get
        {
            return SettlementOwner switch
            {
                PlayerColor.Red => "#FF0000",
                PlayerColor.Blue => "#0000FF",
                PlayerColor.White => "#FFFFFF",
                PlayerColor.Orange => "#FFA500",
                _ => "#808080"
            };
        }
    }

    public VertexViewModel(VertexPosition position)
    {
        Position = position;
        Center = CalculateVertexPosition(position);
        IsClickable = false;
        HasSettlement = false;
        SettlementOwner = null;
        IsCity = false;
    }

    private Point CalculateVertexPosition(VertexPosition vertex)
    {
        // タイルの中心座標を計算
        double tileX = HexSize * (3.0 / 2.0 * vertex.Q);
        double tileY = HexSize * (Math.Sqrt(3) / 2.0 * vertex.Q + Math.Sqrt(3) * vertex.R);

        // 頂点の角度（度）を計算（Direction 0が右上、時計回り）
        double angleInDegrees = 60 * vertex.Direction;
        double angleInRadians = angleInDegrees * Math.PI / 180.0;

        // タイル中心から頂点への相対位置を計算
        double vertexX = tileX + HexSize * Math.Cos(angleInRadians);
        double vertexY = tileY + HexSize * Math.Sin(angleInRadians);

        // Canvas全体の中心オフセット（400, 350）を追加
        return new Point(vertexX + 400, vertexY + 350);
    }
}
