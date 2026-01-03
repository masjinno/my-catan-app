namespace CatanGame.Core.Models;

public enum GamePhase
{
    Setup,
    Playing,
    Ended
}

public enum SetupPhase
{
    PlacingFirstSettlement,
    PlacingFirstRoad,
    PlacingSecondSettlement,
    PlacingSecondRoad,
    Completed
}

public class GameState
{
    public Board Board { get; set; }
    public List<Player> Players { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public GamePhase Phase { get; set; }
    public SetupPhase CurrentSetupPhase { get; set; }
    public int DiceRoll { get; set; }
    public int SetupRound { get; set; } // 0=1巡目, 1=2巡目

    public GameState()
    {
        Board = new Board();
        Players = new List<Player>();
        CurrentPlayerIndex = 0;
        Phase = GamePhase.Setup;
        CurrentSetupPhase = SetupPhase.PlacingFirstSettlement;
        DiceRoll = 0;
        SetupRound = 0;
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

    /// <summary>
    /// 初期配置フェーズで開拓地を配置した後の処理
    /// </summary>
    public void OnSettlementPlacedInSetup()
    {
        if (CurrentSetupPhase == SetupPhase.PlacingFirstSettlement)
        {
            CurrentSetupPhase = SetupPhase.PlacingFirstRoad;
        }
        else if (CurrentSetupPhase == SetupPhase.PlacingSecondSettlement)
        {
            CurrentSetupPhase = SetupPhase.PlacingSecondRoad;
        }
    }

    /// <summary>
    /// 初期配置フェーズで道路を配置した後の処理
    /// </summary>
    public void OnRoadPlacedInSetup()
    {
        if (CurrentSetupPhase == SetupPhase.PlacingFirstRoad)
        {
            // 1巡目：次のプレイヤーへ
            if (CurrentPlayerIndex < Players.Count - 1)
            {
                CurrentPlayerIndex++;
            }
            else
            {
                // 全プレイヤーが1巡目を終えたら2巡目へ（順番は逆）
                SetupRound = 1;
                CurrentSetupPhase = SetupPhase.PlacingSecondSettlement;
            }
        }
        else if (CurrentSetupPhase == SetupPhase.PlacingSecondRoad)
        {
            // 2巡目：前のプレイヤーへ
            if (CurrentPlayerIndex > 0)
            {
                CurrentPlayerIndex--;
                CurrentSetupPhase = SetupPhase.PlacingSecondSettlement;
            }
            else
            {
                // 全プレイヤーが2巡目を終えたらセットアップ完了
                CurrentSetupPhase = SetupPhase.Completed;
                Phase = GamePhase.Playing;
                CurrentPlayerIndex = 0;
            }
        }
    }
}
