namespace CatanGame.Core.Models;

public class Road
{
    public EdgePosition Position { get; set; }
    public Player Owner { get; set; }

    public Road(EdgePosition position, Player owner)
    {
        Position = position;
        Owner = owner;
    }
}
