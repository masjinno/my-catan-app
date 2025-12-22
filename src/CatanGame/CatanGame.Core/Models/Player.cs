namespace CatanGame.Core.Models;

public class Player
{
    public string Name { get; set; }
    public PlayerColor Color { get; set; }
    public Dictionary<ResourceType, int> Resources { get; set; }
    public int VictoryPoints { get; set; }
    public int SettlementCount { get; set; }
    public int CityCount { get; set; }
    public int RoadCount { get; set; }
    public List<DevelopmentCard> DevelopmentCards { get; set; }

    public Player(string name, PlayerColor color)
    {
        Name = name;
        Color = color;
        Resources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 0 },
            { ResourceType.Brick, 0 },
            { ResourceType.Sheep, 0 },
            { ResourceType.Wheat, 0 },
            { ResourceType.Ore, 0 }
        };
        VictoryPoints = 0;
        SettlementCount = 5;
        CityCount = 4;
        RoadCount = 15;
        DevelopmentCards = new List<DevelopmentCard>();
    }

    public bool CanAfford(Dictionary<ResourceType, int> cost)
    {
        return cost.All(kvp => Resources[kvp.Key] >= kvp.Value);
    }

    public void SpendResources(Dictionary<ResourceType, int> cost)
    {
        foreach (var kvp in cost)
        {
            Resources[kvp.Key] -= kvp.Value;
        }
    }

    public void AddResource(ResourceType type, int amount = 1)
    {
        Resources[type] += amount;
    }
}
