namespace CatanGame.Core.Models;

public class Board
{
    public List<HexTile> Tiles { get; set; }
    public Dictionary<VertexPosition, Settlement> Settlements { get; set; }
    public Dictionary<EdgePosition, Road> Roads { get; set; }
    public List<Port> Ports { get; set; }

    public Board()
    {
        Tiles = new List<HexTile>();
        Settlements = new Dictionary<VertexPosition, Settlement>();
        Roads = new Dictionary<EdgePosition, Road>();
        Ports = new List<Port>();
        InitializeBoard();
        InitializePorts();
    }

    private void InitializeBoard()
    {
        // 螺旋状の配置順序（外側の角から中心に向かって時計回り）
        var baseLayout = new[]
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

        var random = new Random();

        // 6つの角のいずれかをランダムに選択してスタート位置とする
        // 角の位置: 0=上, 2=右上, 4=右, 6=右下, 8=下, 10=左下
        int[] cornerIndices = { 0, 2, 4, 6, 8, 10 };
        int startCornerIndex = cornerIndices[random.Next(cornerIndices.Length)];

        // レイアウトを回転させる（スタート位置を先頭にする）
        var layout = new List<dynamic>();

        // 外周のタイル（0-11）を回転
        for (int i = 0; i < 12; i++)
        {
            layout.Add(baseLayout[(startCornerIndex + i) % 12]);
        }

        // 内側のリング（12-17）を回転（外周と同じ回転量）
        int innerRotation = startCornerIndex / 2; // 角のインデックスを内側リングの回転量に変換
        for (int i = 0; i < 6; i++)
        {
            layout.Add(baseLayout[12 + (innerRotation + i) % 6]);
        }

        // 中央（18）はそのまま
        layout.Add(baseLayout[18]);

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

        resources = resources.OrderBy(x => random.Next()).ToList();

        // 数字チップのインデックス
        int numberIndex = 0;

        for (int i = 0; i < layout.Count; i++)
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
        var normalizedPosition = position.GetNormalized();
        Settlements[normalizedPosition] = new Settlement(normalizedPosition, player);
        player.SettlementCount--;
        player.VictoryPoints++;
    }

    public void PlaceRoad(EdgePosition position, Player player)
    {
        var normalizedPosition = position.GetNormalized();
        Roads[normalizedPosition] = new Road(normalizedPosition, player);
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

        // 頂点に隣接する3つの頂点を取得
        // 1. 現在のタイルの隣接頂点（時計回り）
        adjacent.Add(new VertexPosition(position.Q, position.R, (dir + 1) % 6).GetNormalized());

        // 2. 現在のタイルの隣接頂点（反時計回り）
        adjacent.Add(new VertexPosition(position.Q, position.R, (dir + 5) % 6).GetNormalized());

        // 3. 隣接タイルの頂点
        var (neighborQ, neighborR, neighborDir) = GetNeighborTileForVertex(position.Q, position.R, dir);
        adjacent.Add(new VertexPosition(neighborQ, neighborR, neighborDir).GetNormalized());

        return adjacent;
    }

    private (int Q, int R, int direction) GetNeighborTileForVertex(int q, int r, int dir)
    {
        // 六角形の頂点に隣接する第3のタイルの座標を計算
        // 頂点Directionに応じて、どの方向のタイルかを決定
        return dir switch
        {
            0 => (q + 1, r    , (dir + 5) % 6),  // 右のタイル
            1 => (q    , r + 1, (dir + 5) % 6),  // 右下のタイル
            2 => (q - 1, r + 1, (dir + 5) % 6),  // 下のタイル
            3 => (q - 1, r    , (dir + 5) % 6),  // 左のタイル
            4 => (q    , r - 1, (dir + 5) % 6),  // 左上のタイル
            5 => (q + 1, r - 1, (dir + 5) % 6),  // 上のタイル
            _ => (q    , r    , (dir + 5) % 6)
        };
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

    /// <summary>
    /// ボード上の港を初期化する
    /// 港は外周タイルの海に面した辺に配置される
    ///
    /// 港の配置ルール：
    /// - 合計9つの港（一般港3:1が4つ、専門港2:1が5つ）
    /// - 海沿いの頂点は30個（外周タイルの外側の頂点）
    /// - 港は海沿いの隣り合う2頂点（=1つの辺）で1セット
    /// - 9港 × 2頂点/港 = 18頂点が港に使用される
    /// - 残り12頂点は港でない頂点
    /// - 港でない頂点は連続して2つまで（3つ連続は不可）
    /// - 異なる港同士は頂点を共有しない（隣接しない）
    /// </summary>
    private void InitializePorts()
    {
        // 港の配置（外周タイルの外側を向いた辺に配置）
        // 3:1港 x 4セット、2:1港 x 5セット（木、土、羊、麦、鉄）

        // 港の位置（座標と方向）を固定で定義
        var portLocations = new[]
        {
            // 各港は (Q座標, R座標, 辺の方向) で定義される
            // 辺の方向: 0=右下、1=下、2=左下、3=左上、4=上、5=右上
            //
            // 配置パターン：角の港 → 3辺スキップ → 港 → 4辺スキップ → 港 → 3辺スキップ → 次の角の港
            // このパターンで島を一周して合計9つの港を配置

            new { Q = 0, R = -2, Dir = 4 },   // 港1：上の角タイル (0,-2) の上の辺
            new { Q = 1, R = -2, Dir = 5 },   // 港2：右上辺タイル (1,-2) の右上の辺
            new { Q = 2, R = -1, Dir = 5 },   // 港3：右辺のタイル (2,-1) の右上の辺
            new { Q = 2, R = 0, Dir = 0 },    // 港4：右下角タイル (2,0) の右下の辺
            new { Q = 1, R = 1, Dir = 1 },    // 港5：右下辺タイル (1,1) の下の辺
            new { Q = -1, R = 2, Dir = 1 },   // 港6：左下辺タイル (-1,2) の下の辺
            new { Q = -2, R = 2, Dir = 2 },   // 港7：左下角タイル (-2,2) の左下の辺
            new { Q = -2, R = 1, Dir = 3 },   // 港8：左辺のタイル（-2,1) の左上の辺
            new { Q = -1, R = -1, Dir = 3 }   // 港9：左上辺タイル (-1,-1) の左上の辺
        };

        // 港の種類をランダムにシャッフル
        // 一般港（3:1）x 4つ、専門港（2:1）x 5つ（木、土、羊、麦、鉄）
        // 制約：一般港は連続して2つまで（3つ以上連続しない）
        var portTypes = new List<PortType>
        {
            PortType.Generic, PortType.Generic, PortType.Generic, PortType.Generic,
            PortType.Wood, PortType.Brick, PortType.Sheep, PortType.Wheat, PortType.Ore
        };

        var random = new Random();
        List<PortType> shuffledPortTypes;

        // 一般港が3つ以上連続しない配置を見つけるまでシャッフルを繰り返す
        int maxAttempts = 1000;
        int attempts = 0;
        do
        {
            shuffledPortTypes = portTypes.OrderBy(_ => random.Next()).ToList();
            attempts++;

            if (attempts >= maxAttempts)
            {
                // 万が一見つからない場合は、強制的に調整
                shuffledPortTypes = AdjustPortTypes(portTypes, random);
                break;
            }
        }
        while (HasThreeConsecutiveGenericPorts(shuffledPortTypes));

        // 港リストに追加（位置は固定、種類はランダム）
        for (int i = 0; i < portLocations.Length; i++)
        {
            var location = portLocations[i];
            Ports.Add(new Port(location.Q, location.R, location.Dir, shuffledPortTypes[i]));
        }
    }

    /// <summary>
    /// 一般港が3つ以上連続しているかチェック
    /// 港は環状に配置されているため、最後と最初も繋がっていることを考慮
    /// </summary>
    private bool HasThreeConsecutiveGenericPorts(List<PortType> portTypes)
    {
        int count = portTypes.Count;

        for (int i = 0; i < count; i++)
        {
            // 現在の港とその次の2つの港をチェック（環状配置を考慮）
            if (portTypes[i] == PortType.Generic &&
                portTypes[(i + 1) % count] == PortType.Generic &&
                portTypes[(i + 2) % count] == PortType.Generic)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 一般港が3つ以上連続しないように強制的に調整
    /// </summary>
    private List<PortType> AdjustPortTypes(List<PortType> portTypes, Random random)
    {
        var result = portTypes.OrderBy(_ => random.Next()).ToList();
        int count = result.Count;

        // 最大100回の調整試行
        for (int attempt = 0; attempt < 100; attempt++)
        {
            bool adjusted = false;

            for (int i = 0; i < count; i++)
            {
                // 一般港が3つ連続している箇所を見つける
                if (result[i] == PortType.Generic &&
                    result[(i + 1) % count] == PortType.Generic &&
                    result[(i + 2) % count] == PortType.Generic)
                {
                    // 3つ目の一般港と、専門港を交換
                    int swapIndex = (i + 2) % count;
                    for (int j = 0; j < count; j++)
                    {
                        if (result[j] != PortType.Generic)
                        {
                            // 交換
                            var temp = result[swapIndex];
                            result[swapIndex] = result[j];
                            result[j] = temp;
                            adjusted = true;
                            break;
                        }
                    }

                    if (adjusted)
                        break;
                }
            }

            // 調整が不要になったらループを抜ける
            if (!adjusted || !HasThreeConsecutiveGenericPorts(result))
                break;
        }

        return result;
    }
}
