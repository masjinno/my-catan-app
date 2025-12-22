namespace CatanGame.Core.Models;

public enum DevelopmentCardType
{
    Knight,
    VictoryPoint,
    RoadBuilding,
    YearOfPlenty,
    Monopoly
}

public class DevelopmentCard
{
    public DevelopmentCardType Type { get; set; }
    public bool HasBeenPlayed { get; set; }

    public DevelopmentCard(DevelopmentCardType type)
    {
        Type = type;
        HasBeenPlayed = false;
    }
}
