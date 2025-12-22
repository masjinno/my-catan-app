using CatanGame.Core.Models;
using CatanGame.App.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CatanGame.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private GameState _gameState;
    private const double HexSize = 60;

    public ObservableCollection<HexTileViewModel> HexTiles { get; }

    public ICommand NewGameCommand { get; }
    public ICommand RollDiceCommand { get; }

    private string _currentPlayerName;
    public string CurrentPlayerName
    {
        get => _currentPlayerName;
        set => SetProperty(ref _currentPlayerName, value);
    }

    private int _diceRoll;
    public int DiceRoll
    {
        get => _diceRoll;
        set => SetProperty(ref _diceRoll, value);
    }

    public MainViewModel()
    {
        _gameState = new GameState();
        _currentPlayerName = "";
        HexTiles = new ObservableCollection<HexTileViewModel>();

        NewGameCommand = new RelayCommand(_ => StartNewGame());
        RollDiceCommand = new RelayCommand(_ => RollDice(), _ => CanRollDice());

        StartNewGame();
    }

    private void StartNewGame()
    {
        _gameState = new GameState();

        _gameState.Players.Add(new Player("プレイヤー1", PlayerColor.Red));
        _gameState.Players.Add(new Player("プレイヤー2", PlayerColor.Blue));
        _gameState.Players.Add(new Player("プレイヤー3", PlayerColor.White));
        _gameState.Players.Add(new Player("プレイヤー4", PlayerColor.Orange));

        _gameState.Phase = GamePhase.Playing;

        HexTiles.Clear();
        foreach (var tile in _gameState.Board.Tiles)
        {
            HexTiles.Add(new HexTileViewModel(tile, HexSize));
        }

        UpdateCurrentPlayer();
    }

    private void RollDice()
    {
        _gameState.RollDice();
        DiceRoll = _gameState.DiceRoll;

        _gameState.DistributeResources(DiceRoll);

        var winner = _gameState.CheckWinner();
        if (winner != null)
        {
            CurrentPlayerName = $"{winner.Name} の勝利！";
            _gameState.Phase = GamePhase.Ended;
        }
    }

    private bool CanRollDice()
    {
        return _gameState.Phase == GamePhase.Playing;
    }

    private void UpdateCurrentPlayer()
    {
        CurrentPlayerName = _gameState.CurrentPlayer.Name;
    }
}
