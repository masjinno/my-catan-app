using CatanGame.Core.Models;
using CatanGame.App.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CatanGame.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private GameState _gameState;
    private const double HexSize = 52;

    public ObservableCollection<HexTileViewModel> HexTiles { get; }
    public ObservableCollection<PortViewModel> Ports { get; }
    public ObservableCollection<VertexViewModel> Vertices { get; }
    public ObservableCollection<EdgeViewModel> Edges { get; }
    public ObservableCollection<PlayerResourceViewModel> PlayersResources { get; }

    public ICommand NewGameCommand { get; }
    public ICommand RollDiceCommand { get; }
    public ICommand VertexClickCommand { get; }
    public ICommand EdgeClickCommand { get; }

    private string _currentPlayerName;
    public string CurrentPlayerName
    {
        get => _currentPlayerName;
        set => SetProperty(ref _currentPlayerName, value);
    }

    private string _currentPlayerColorBrush;
    public string CurrentPlayerColorBrush
    {
        get => _currentPlayerColorBrush;
        set => SetProperty(ref _currentPlayerColorBrush, value);
    }

    private int _diceRoll;
    public int DiceRoll
    {
        get => _diceRoll;
        set => SetProperty(ref _diceRoll, value);
    }

    private string _setupPhaseMessage;
    public string SetupPhaseMessage
    {
        get => _setupPhaseMessage;
        set => SetProperty(ref _setupPhaseMessage, value);
    }

    private string _currentPlayerResources;
    public string CurrentPlayerResources
    {
        get => _currentPlayerResources;
        set => SetProperty(ref _currentPlayerResources, value);
    }

    private VertexPosition? _lastPlacedSettlement;

    public MainViewModel()
    {
        _gameState = new GameState();
        _currentPlayerName = "";
        _currentPlayerColorBrush = "";
        _setupPhaseMessage = "";
        _currentPlayerResources = "";
        HexTiles = new ObservableCollection<HexTileViewModel>();
        Ports = new ObservableCollection<PortViewModel>();
        Vertices = new ObservableCollection<VertexViewModel>();
        Edges = new ObservableCollection<EdgeViewModel>();
        PlayersResources = new ObservableCollection<PlayerResourceViewModel>();

        NewGameCommand = new RelayCommand(_ => StartNewGame());
        RollDiceCommand = new RelayCommand(_ => RollDice(), _ => CanRollDice());
        VertexClickCommand = new RelayCommand(param => OnVertexClick(param), _ => CanClickVertex());
        EdgeClickCommand = new RelayCommand(param => OnEdgeClick(param), _ => CanClickEdge());

        StartNewGame();
    }

    private void StartNewGame()
    {
        _gameState = new GameState();

        _gameState.Players.Add(new Player("プレイヤー1", PlayerColor.Red));
        _gameState.Players.Add(new Player("プレイヤー2", PlayerColor.Blue));
        _gameState.Players.Add(new Player("プレイヤー3", PlayerColor.White));
        _gameState.Players.Add(new Player("プレイヤー4", PlayerColor.Orange));

        _gameState.Phase = GamePhase.Setup;
        _gameState.CurrentSetupPhase = SetupPhase.PlacingFirstSettlement;

        HexTiles.Clear();
        foreach (var tile in _gameState.Board.Tiles)
        {
            HexTiles.Add(new HexTileViewModel(tile, HexSize));
        }

        Ports.Clear();
        foreach (var port in _gameState.Board.Ports)
        {
            Ports.Add(new PortViewModel(port));
        }

        InitializeVertices();
        InitializeEdges();

        InitializePlayersResources();

        UpdateCurrentPlayer();
        UpdateSetupPhaseMessage();
        UpdateClickableVertices();
    }

    private void InitializePlayersResources()
    {
        PlayersResources.Clear();
        foreach (var player in _gameState.Players)
        {
            var playerResourceVM = new PlayerResourceViewModel
            {
                PlayerName = player.Name,
                ColorBrush = player.Color switch
                {
                    PlayerColor.Red => "#D32F2F",
                    PlayerColor.Blue => "#1976D2",
                    PlayerColor.Orange => "#F57C00",
                    PlayerColor.White => "#FFFFFF",
                    _ => "#000000"
                }
            };
            UpdatePlayerResourcesText(playerResourceVM, player);
            PlayersResources.Add(playerResourceVM);
        }
    }

    private void UpdatePlayerResourcesText(PlayerResourceViewModel vm, Player player)
    {
        var resources = player.Resources;
        vm.Resources = $"木: {resources[ResourceType.Wood]}  " +
                      $"土: {resources[ResourceType.Brick]}  " +
                      $"羊: {resources[ResourceType.Sheep]}  " +
                      $"麦: {resources[ResourceType.Wheat]}  " +
                      $"鉄: {resources[ResourceType.Ore]}";
    }

    private void InitializeVertices()
    {
        Vertices.Clear();
        var vertexPositions = new HashSet<(int Q, int R, int Dir)>();

        // 各タイルの6つの頂点を生成（正規化された座標のみ）
        foreach (var tile in _gameState.Board.Tiles)
        {
            for (int dir = 0; dir < 6; dir++)
            {
                var position = new VertexPosition(tile.Q, tile.R, dir);
                var normalized = position.GetNormalized();
                var key = (normalized.Q, normalized.R, normalized.Direction);

                if (!vertexPositions.Contains(key))
                {
                    vertexPositions.Add(key);
                    var vertex = new VertexViewModel(normalized);
                    Vertices.Add(vertex);
                }
            }
        }
    }

    private void InitializeEdges()
    {
        Edges.Clear();
        var edgePositions = new HashSet<(int Q, int R, int Dir)>();

        // 各タイルの6つの辺を生成（正規化された座標のみ）
        foreach (var tile in _gameState.Board.Tiles)
        {
            for (int dir = 0; dir < 6; dir++)
            {
                var position = new EdgePosition(tile.Q, tile.R, dir);
                var normalized = position.GetNormalized();
                var key = (normalized.Q, normalized.R, normalized.Direction);

                if (!edgePositions.Contains(key))
                {
                    edgePositions.Add(key);
                    var edge = new EdgeViewModel(normalized);
                    Edges.Add(edge);
                }
            }
        }
    }

    private void RollDice()
    {
        _gameState.RollDice();
        DiceRoll = _gameState.DiceRoll;

        _gameState.DistributeResources(DiceRoll);
        UpdateCurrentPlayerResources();

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
        CurrentPlayerColorBrush = _gameState.CurrentPlayer.Color switch
        {
            PlayerColor.Red => "#D32F2F",
            PlayerColor.Blue => "#1976D2",
            PlayerColor.Orange => "#F57C00",
            PlayerColor.White => "#FFFFFF",
            _ => "#000000"
        };
        UpdateCurrentPlayerResources();
    }

    private void UpdateCurrentPlayerResources()
    {
        // 全プレイヤーの手札情報を更新
        for (int i = 0; i < _gameState.Players.Count; i++)
        {
            var player = _gameState.Players[i];
            var vm = PlayersResources[i];
            UpdatePlayerResourcesText(vm, player);
        }

        // 現在のプレイヤーの手札情報も更新（互換性のため残す）
        var currentPlayer = _gameState.CurrentPlayer;
        var resources = currentPlayer.Resources;
        CurrentPlayerResources = $"木: {resources[ResourceType.Wood]}  " +
                                 $"土: {resources[ResourceType.Brick]}  " +
                                 $"羊: {resources[ResourceType.Sheep]}  " +
                                 $"麦: {resources[ResourceType.Wheat]}  " +
                                 $"鉄: {resources[ResourceType.Ore]}";
    }

    private void UpdateSetupPhaseMessage()
    {
        if (_gameState.Phase != GamePhase.Setup)
        {
            SetupPhaseMessage = "";
            return;
        }

        SetupPhaseMessage = _gameState.CurrentSetupPhase switch
        {
            SetupPhase.PlacingFirstSettlement => "開拓地を配置してください（1巡目）",
            SetupPhase.PlacingFirstRoad => "道路を配置してください（1巡目）",
            SetupPhase.PlacingSecondSettlement => "開拓地を配置してください（2巡目）",
            SetupPhase.PlacingSecondRoad => "道路を配置してください（2巡目）",
            _ => ""
        };
    }

    private void UpdateClickableVertices()
    {
        foreach (var vertex in Vertices)
        {
            vertex.IsClickable = _gameState.Phase == GamePhase.Setup &&
                                (_gameState.CurrentSetupPhase == SetupPhase.PlacingFirstSettlement ||
                                 _gameState.CurrentSetupPhase == SetupPhase.PlacingSecondSettlement) &&
                                _gameState.Board.CanPlaceSettlement(vertex.Position, _gameState.CurrentPlayer, true);
        }
    }

    private void UpdateClickableEdges()
    {
        // 最後に配置した開拓地に隣接する辺のリストを事前に取得
        List<EdgePosition>? adjacentEdges = null;
        if (_gameState.Phase == GamePhase.Setup &&
            (_gameState.CurrentSetupPhase == SetupPhase.PlacingFirstRoad ||
             _gameState.CurrentSetupPhase == SetupPhase.PlacingSecondRoad) &&
            _lastPlacedSettlement != null)
        {
            adjacentEdges = GetAdjacentEdgesToVertex(_lastPlacedSettlement);
        }

        foreach (var edge in Edges)
        {
            bool isClickable = false;

            if (adjacentEdges != null)
            {
                isClickable = adjacentEdges.Any(e => e.Equals(edge.Position)) &&
                              !_gameState.Board.Roads.ContainsKey(edge.Position);
            }

            edge.IsClickable = isClickable;
        }
    }

    private List<EdgePosition> GetAdjacentEdgesToVertex(VertexPosition vertex)
    {
        var edges = new List<EdgePosition>();
        int dir = vertex.Direction;

        // 頂点に隣接する3つの辺を取得
        // 頂点Directionは、タイル中心から見た頂点の角度を示す（0=右、時計回り）
        // 辺Directionは、タイルの6つの辺を示す（0=右下、1=下、2=左下、3=左上、4=上、5=右上）

        // 1. 現在のタイルの辺（頂点から時計回りの辺）
        edges.Add(new EdgePosition(vertex.Q, vertex.R, dir).GetNormalized());

        // 2. 現在のタイルの辺（頂点から反時計回りの辺）
        edges.Add(new EdgePosition(vertex.Q, vertex.R, (dir + 5) % 6).GetNormalized());

        // 3. 隣接タイルの辺
        var (neighborQ, neighborR, neighborDir) = GetNeighborTileForVertex(vertex.Q, vertex.R, dir);
        edges.Add(new EdgePosition(neighborQ, neighborR, neighborDir).GetNormalized());

        return edges;
    }

    private (int Q, int R, int direction) GetNeighborTileForVertex(int q, int r, int dir)
    {
        // 六角形の頂点に隣接する第3のタイルの座標を計算
        // 頂点Directionに応じて、どの方向のタイルかを決定
        return dir switch
        {
            0 => (q + 1, r    , (dir + 4) % 6),  // 右のタイル
            1 => (q    , r + 1, (dir + 4) % 6),  // 右下のタイル
            2 => (q - 1, r + 1, (dir + 4) % 6),  // 下のタイル
            3 => (q - 1, r    , (dir + 4) % 6),  // 左のタイル
            4 => (q    , r - 1, (dir + 4) % 6),  // 左上のタイル
            5 => (q + 1, r - 1, (dir + 4) % 6),  // 上のタイル
            _ => (q    , r    , (dir + 4) % 6)
        };
    }

    private bool CanClickVertex()
    {
        return _gameState.Phase == GamePhase.Setup &&
               (_gameState.CurrentSetupPhase == SetupPhase.PlacingFirstSettlement ||
                _gameState.CurrentSetupPhase == SetupPhase.PlacingSecondSettlement);
    }

    private bool CanClickEdge()
    {
        return _gameState.Phase == GamePhase.Setup &&
               (_gameState.CurrentSetupPhase == SetupPhase.PlacingFirstRoad ||
                _gameState.CurrentSetupPhase == SetupPhase.PlacingSecondRoad);
    }

    private void OnVertexClick(object? parameter)
    {
        if (parameter is not VertexViewModel vertexVM || !vertexVM.IsClickable)
            return;

        // 開拓地を配置
        _gameState.Board.PlaceSettlement(vertexVM.Position, _gameState.CurrentPlayer);
        vertexVM.HasSettlement = true;
        vertexVM.SettlementOwner = _gameState.CurrentPlayer.Color;
        vertexVM.IsClickable = false;

        // 最後に配置した開拓地を記憶
        _lastPlacedSettlement = vertexVM.Position;

        // 2巡目の場合、初期資源を配布（フェーズ進行の前にチェック）
        if (_gameState.CurrentSetupPhase == SetupPhase.PlacingSecondSettlement)
        {
            DistributeInitialResources(vertexVM.Position);
            UpdateCurrentPlayerResources();
        }

        // 初期配置フェーズの進行
        _gameState.OnSettlementPlacedInSetup();

        UpdateSetupPhaseMessage();
        UpdateClickableVertices();
        UpdateClickableEdges();
    }

    private void OnEdgeClick(object? parameter)
    {
        if (parameter is not EdgeViewModel edgeVM || !edgeVM.IsClickable)
            return;

        // 道路を配置
        _gameState.Board.PlaceRoad(edgeVM.Position, _gameState.CurrentPlayer);
        edgeVM.HasRoad = true;
        edgeVM.RoadOwner = _gameState.CurrentPlayer.Color;
        edgeVM.IsClickable = false;

        // 最後に配置した開拓地をリセット
        _lastPlacedSettlement = null;

        // 初期配置フェーズの進行
        _gameState.OnRoadPlacedInSetup();

        UpdateSetupPhaseMessage();
        UpdateClickableVertices();
        UpdateClickableEdges();
        UpdateCurrentPlayer();
    }

    private void DistributeInitialResources(VertexPosition settlementPosition)
    {
        // 開拓地に隣接するタイルから初期資源を取得
        var adjacentTiles = _gameState.Board.Tiles
            .Where(t => IsVertexAdjacentToTile(settlementPosition, t))
            .ToList();

        foreach (var tile in adjacentTiles)
        {
            if (tile.ResourceType != ResourceType.Desert)
            {
                _gameState.CurrentPlayer.AddResource(tile.ResourceType, 1);
            }
        }
    }

    private bool IsVertexAdjacentToTile(VertexPosition vertex, HexTile tile)
    {
        // 頂点は3つのタイルに隣接している
        // 正規化された頂点座標から、隣接する3つのタイルの座標を取得
        var normalizedVertex = vertex.GetNormalized();

        // 1つ目: 頂点座標のタイル
        if (normalizedVertex.Q == tile.Q && normalizedVertex.R == tile.R)
            return true;

        // 2つ目と3つ目: VertexPosition.GetNormalized() のロジックから隣接タイルを計算
        var adjacentTiles = new List<(int Q, int R)>();

        switch (normalizedVertex.Direction)
        {
            case 0: // 右の頂点
                adjacentTiles.Add((normalizedVertex.Q + 1, normalizedVertex.R - 1)); // 右上のタイル
                adjacentTiles.Add((normalizedVertex.Q + 1, normalizedVertex.R));     // 右下のタイル
                break;
            case 1: // 右下の頂点
                adjacentTiles.Add((normalizedVertex.Q + 1, normalizedVertex.R));     // 右下のタイル
                adjacentTiles.Add((normalizedVertex.Q, normalizedVertex.R + 1));     // 下のタイル
                break;
            case 2: // 左下の頂点
                adjacentTiles.Add((normalizedVertex.Q, normalizedVertex.R + 1));     // 下のタイル
                adjacentTiles.Add((normalizedVertex.Q - 1, normalizedVertex.R + 1)); // 左下のタイル
                break;
            case 3: // 左の頂点
                adjacentTiles.Add((normalizedVertex.Q - 1, normalizedVertex.R + 1)); // 左下のタイル
                adjacentTiles.Add((normalizedVertex.Q - 1, normalizedVertex.R));     // 左上のタイル
                break;
            case 4: // 左上の頂点
                adjacentTiles.Add((normalizedVertex.Q - 1, normalizedVertex.R));     // 左上のタイル
                adjacentTiles.Add((normalizedVertex.Q, normalizedVertex.R - 1));     // 上のタイル
                break;
            case 5: // 上の頂点
                adjacentTiles.Add((normalizedVertex.Q, normalizedVertex.R - 1));     // 左上のタイル
                adjacentTiles.Add((normalizedVertex.Q + 1, normalizedVertex.R - 1)); // 右上のタイル
                break;
        }

        return adjacentTiles.Any(t => t.Q == tile.Q && t.R == tile.R);
    }
}
