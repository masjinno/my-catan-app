namespace CatanGame.Core.Models;

public class Settlement
{
    public VertexPosition Position { get; set; }
    public Player Owner { get; set; }
    public bool IsCity { get; set; }

    public Settlement(VertexPosition position, Player owner, bool isCity = false)
    {
        Position = position;
        Owner = owner;
        IsCity = isCity;
    }

    public int GetVictoryPoints()
    {
        return IsCity ? 2 : 1;
    }
}
