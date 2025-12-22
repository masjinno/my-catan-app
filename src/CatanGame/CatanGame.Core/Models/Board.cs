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
        var layout = new[]
        {
            new { Q = 0, R = -2 },
            new { Q = 1, R = -2 },
            new { Q = 2, R = -2 },
            new { Q = -1, R = -1 },
            new { Q = 0, R = -1 },
            new { Q = 1, R = -1 },
            new { Q = 2, R = -1 },
            new { Q = -2, R = 0 },
            new { Q = -1, R = 0 },
            new { Q = 0, R = 0 },
            new { Q = 1, R = 0 },
            new { Q = 2, R = 0 },
            new { Q = -2, R = 1 },
            new { Q = -1, R = 1 },
            new { Q = 0, R = 1 },
            new { Q = 1, R = 1 },
            new { Q = -2, R = 2 },
            new { Q = -1, R = 2 },
            new { Q = 0, R = 2 }
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

        var numbers = new List<int?>
        {
            2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12, null
        };

        var random = new Random();
        resources = resources.OrderBy(x => random.Next()).ToList();
        numbers = numbers.OrderBy(x => random.Next()).ToList();

        for (int i = 0; i < layout.Length; i++)
        {
            var pos = layout[i];
            var resource = resources[i];
            var number = resource == ResourceType.Desert ? null : numbers[i];

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
