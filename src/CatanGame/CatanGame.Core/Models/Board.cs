namespace CatanGame.Core.Models;

public class Board
{
    public List<HexTile> Tiles { get; set; }
    public Dictionary<VertexPosition, Settlement> Settlements { get; set; }
    public Dictionary<EdgePosition, Road> Roads { get; set; }

    public Board()
    {
        Tiles = new List<HexTile>();
        Settlements = new Dictionary<VertexPosition, Settlement>();
        Roads = new Dictionary<EdgePosition, Road>();
        InitializeBoard();
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
}
