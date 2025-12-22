namespace CatanGame.Core.Models;

public enum GamePhase
{
    Setup,
    Playing,
    Ended
}

public class GameState
{
    public Board Board { get; set; }
    public List<Player> Players { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public GamePhase Phase { get; set; }
    public int DiceRoll { get; set; }

    public GameState()
    {
        Board = new Board();
        Players = new List<Player>();
        CurrentPlayerIndex = 0;
        Phase = GamePhase.Setup;
        DiceRoll = 0;
    }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public void NextTurn()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
    }

    public void RollDice()
    {
        var random = new Random();
        int die1 = random.Next(1, 7);
        int die2 = random.Next(1, 7);
        DiceRoll = die1 + die2;
    }

    public void DistributeResources(int diceRoll)
    {
        if (diceRoll == 7)
        {
            return;
        }

        var tilesToDistribute = Board.Tiles
            .Where(t => t.NumberToken == diceRoll && !t.HasRobber)
            .ToList();

        foreach (var tile in tilesToDistribute)
        {
            var settlements = Board.GetSettlementsOnTile(tile);
            foreach (var settlement in settlements)
            {
                int amount = settlement.IsCity ? 2 : 1;
                settlement.Owner.AddResource(tile.ResourceType, amount);
            }
        }
    }

    public Player? CheckWinner()
    {
        return Players.FirstOrDefault(p => p.VictoryPoints >= 10);
    }
}
