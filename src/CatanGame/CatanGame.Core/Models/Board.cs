namespace CatanGame.Core.Models;

public class Board
{
    public List<HexTile> Tiles { get; set; }
    public Dictionary<VertexPosition, Settlement> Settlements { get; set; }
    public Dictionary<EdgePosition, Road> Roads { get; set; }
    public List<Port> Ports { get; set; }

    public Board()
    {
        Tiles = new List<HexTile>();
        Settlements = new Dictionary<VertexPosition, Settlement>();
        Roads = new Dictionary<EdgePosition, Road>();
        Ports = new List<Port>();
        InitializeBoard();
        InitializePorts();
    }

    private void InitializeBoard()
    {
        // 螺旋状の配置順序（外側の角から中心に向かって時計回り）
        var layout = new[]
        {
            // 外周（角から時計回り）
            new { Q = 0, R = -2 },   // 0: 上の角
            new { Q = 1, R = -2 },   // 1
            new { Q = 2, R = -2 },   // 2: 右上の角
            new { Q = 2, R = -1 },   // 3
            new { Q = 2, R = 0 },    // 4: 右の角
            new { Q = 1, R = 1 },    // 5
            new { Q = 0, R = 2 },    // 6: 右下の角
            new { Q = -1, R = 2 },   // 7
            new { Q = -2, R = 2 },   // 8: 下の角
            new { Q = -2, R = 1 },   // 9
            new { Q = -2, R = 0 },   // 10: 左下の角
            new { Q = -1, R = -1 },  // 11
            // 内側のリング
            new { Q = 0, R = -1 },   // 12
            new { Q = 1, R = -1 },   // 13
            new { Q = 1, R = 0 },    // 14
            new { Q = 0, R = 1 },    // 15
            new { Q = -1, R = 1 },   // 16
            new { Q = -1, R = 0 },   // 17
            // 中央
            new { Q = 0, R = 0 }     // 18
        };

        var resources = new List<ResourceType>
        {
            ResourceType.Wood, ResourceType.Wood, ResourceType.Wood, ResourceType.Wood,
            ResourceType.Brick, ResourceType.Brick, ResourceType.Brick,
            ResourceType.Sheep, ResourceType.Sheep, ResourceType.Sheep, ResourceType.Sheep,
            ResourceType.Wheat, ResourceType.Wheat, ResourceType.Wheat, ResourceType.Wheat,
            ResourceType.Ore, ResourceType.Ore, ResourceType.Ore,
            ResourceType.Desert
        };

        // 数字チップの配置順序（砂漠はスキップ）
        var numberSequence = new List<int> { 5, 2, 6, 3, 8, 10, 9, 12, 11, 4, 8, 10, 9, 4, 5, 6, 3, 11 };

        var random = new Random();
        resources = resources.OrderBy(x => random.Next()).ToList();

        // 数字チップのインデックス
        int numberIndex = 0;

        for (int i = 0; i < layout.Length; i++)
        {
            var pos = layout[i];
            var resource = resources[i];
            int? number = null;

            // 砂漠でない場合のみ数字チップを配置
            if (resource != ResourceType.Desert)
            {
                number = numberSequence[numberIndex];
                numberIndex++;
            }

            Tiles.Add(new HexTile(pos.Q, pos.R, resource, number));
        }
    }

    public bool CanPlaceSettlement(VertexPosition position, Player player, bool isInitialPlacement = false)
    {
        if (Settlements.ContainsKey(position))
            return false;

        if (!isInitialPlacement)
        {
            bool hasAdjacentRoad = GetAdjacentEdges(position)
                .Any(edge => Roads.ContainsKey(edge) && Roads[edge].Owner == player);

            if (!hasAdjacentRoad)
                return false;
        }

        var adjacentVertices = GetAdjacentVertices(position);
        return !adjacentVertices.Any(v => Settlements.ContainsKey(v));
    }

    public bool CanPlaceRoad(EdgePosition position, Player player)
    {
        if (Roads.ContainsKey(position))
            return false;

        var adjacentVertices = GetVerticesForEdge(position);
        bool hasAdjacentStructure = adjacentVertices.Any(v =>
            Settlements.ContainsKey(v) && Settlements[v].Owner == player);

        if (hasAdjacentStructure)
            return true;

        var adjacentEdges = GetAdjacentEdgesToEdge(position);
        return adjacentEdges.Any(e => Roads.ContainsKey(e) && Roads[e].Owner == player);
    }

    public void PlaceSettlement(VertexPosition position, Player player)
    {
        Settlements[position] = new Settlement(position, player);
        player.SettlementCount--;
        player.VictoryPoints++;
    }

    public void PlaceRoad(EdgePosition position, Player player)
    {
        Roads[position] = new Road(position, player);
        player.RoadCount--;
    }

    public void UpgradeToCity(VertexPosition position)
    {
        if (Settlements.TryGetValue(position, out var settlement) && !settlement.IsCity)
        {
            settlement.IsCity = true;
            settlement.Owner.SettlementCount++;
            settlement.Owner.CityCount--;
            settlement.Owner.VictoryPoints++;
        }
    }

    private List<VertexPosition> GetAdjacentVertices(VertexPosition position)
    {
        var adjacent = new List<VertexPosition>();
        int dir = position.Direction;

        adjacent.Add(new VertexPosition(position.Q, position.R, (dir + 1) % 6));
        adjacent.Add(new VertexPosition(position.Q, position.R, (dir + 5) % 6));

        return adjacent;
    }

    private List<EdgePosition> GetAdjacentEdges(VertexPosition position)
    {
        var edges = new List<EdgePosition>();
        int dir = position.Direction;

        edges.Add(new EdgePosition(position.Q, position.R, dir));
        edges.Add(new EdgePosition(position.Q, position.R, (dir + 5) % 6));

        return edges;
    }

    private List<VertexPosition> GetVerticesForEdge(EdgePosition position)
    {
        return new List<VertexPosition>
        {
            new VertexPosition(position.Q, position.R, position.Direction),
            new VertexPosition(position.Q, position.R, (position.Direction + 1) % 6)
        };
    }

    private List<EdgePosition> GetAdjacentEdgesToEdge(EdgePosition position)
    {
        var edges = new List<EdgePosition>();
        int dir = position.Direction;

        edges.Add(new EdgePosition(position.Q, position.R, (dir + 1) % 6));
        edges.Add(new EdgePosition(position.Q, position.R, (dir + 5) % 6));

        return edges;
    }

    public List<Settlement> GetSettlementsOnTile(HexTile tile)
    {
        return Settlements.Values
            .Where(s => IsVertexAdjacentToTile(s.Position, tile))
            .ToList();
    }

    private bool IsVertexAdjacentToTile(VertexPosition vertex, HexTile tile)
    {
        return vertex.Q == tile.Q && vertex.R == tile.R;
    }

    /// <summary>
    /// ボード上の港を初期化する
    /// 港は外周タイルの海に面した辺に配置される
    ///
    /// 港の配置ルール：
    /// - 合計9つの港（一般港3:1が4つ、専門港2:1が5つ）
    /// - 海沿いの頂点は30個（外周タイルの外側の頂点）
    /// - 港は海沿いの隣り合う2頂点（=1つの辺）で1セット
    /// - 9港 × 2頂点/港 = 18頂点が港に使用される
    /// - 残り12頂点は港でない頂点
    /// - 港でない頂点は連続して2つまで（3つ連続は不可）
    /// - 異なる港同士は頂点を共有しない（隣接しない）
    /// </summary>
    private void InitializePorts()
    {
        // 港の配置（外周タイルの外側を向いた辺に配置）
        // 3:1港 x 4セット、2:1港 x 5セット（木、土、羊、麦、鉄）

        var portPlacements = new[]
        {
            // 各港は (Q座標, R座標, 辺の方向, 港の種類) で定義される
            // 辺の方向: 0=右下、1=下、2=左下、3=左上、4=上、5=右上
            //
            // 配置パターン：角の港 → 3辺スキップ → 港 → 4辺スキップ → 港 → 3辺スキップ → 次の角の港
            // このパターンで島を一周して合計9つの港を配置

            // 港1：上の角タイル (0,-2) の上の辺
            new { Q = 0, R = -2, Dir = 4, Type = PortType.Generic },

            // 港2：右上辺タイル (1,-2) の右上の辺
            new { Q = 1, R = -2, Dir = 5, Type = PortType.Wood },

            // 港3：右辺のタイル (2,-1) の右上の辺
            new { Q = 2, R = -1, Dir = 5, Type = PortType.Generic },

            // 港4：右下角タイル (2,0) の右下の辺
            new { Q = 2, R = 0, Dir = 0, Type = PortType.Sheep },

            // 港5：右下辺タイル (1,1) の下の辺
            new { Q = 1, R = 1, Dir = 1, Type = PortType.Generic },

            // 港6：左下辺タイル (-1,2) の下の辺
            new { Q = -1, R = 2, Dir = 1, Type = PortType.Wheat },

            // 港7：左下角タイル (-2,2) の左下の辺
            new { Q = -2, R = 2, Dir = 2, Type = PortType.Brick },

            // 港8：左辺のタイル（-2,1) の左上の辺
            new { Q = -2, R = 1, Dir = 3, Type = PortType.Generic },

            // 港9：左上辺タイル (-1,-1) の左上の辺
            new { Q = -1, R = -1, Dir = 3, Type = PortType.Ore }
        };

        // 港リストに追加
        foreach (var placement in portPlacements)
        {
            Ports.Add(new Port(placement.Q, placement.R, placement.Dir, placement.Type));
        }
    }
}
