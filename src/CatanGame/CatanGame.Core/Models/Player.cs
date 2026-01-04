namespace CatanGame.Core.Models;

/// <summary>
/// プレイヤーを表すクラス
/// 資源、勝利点、建築物数などプレイヤーの状態を管理
/// </summary>
public class Player
{
    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// プレイヤーの色（赤、青、白、オレンジなど）
    /// </summary>
    public PlayerColor Color { get; set; }

    /// <summary>
    /// 所持している資源カードの枚数
    /// キー: 資源の種類、値: 枚数
    /// </summary>
    public Dictionary<ResourceType, int> Resources { get; set; }

    /// <summary>
    /// 現在の勝利点
    /// 10点以上で勝利
    /// </summary>
    public int VictoryPoints { get; set; }

    /// <summary>
    /// 残りの建設可能な開拓地の数
    /// 初期値: 5（最大5個まで建設可能）
    /// </summary>
    public int SettlementCount { get; set; }

    /// <summary>
    /// 残りの建設可能な都市の数
    /// 初期値: 4（最大4個まで建設可能）
    /// </summary>
    public int CityCount { get; set; }

    /// <summary>
    /// 残りの建設可能な道路の数
    /// 初期値: 15（最大15本まで建設可能）
    /// </summary>
    public int RoadCount { get; set; }

    /// <summary>
    /// 所持している発展カードのリスト
    /// </summary>
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

    /// <summary>
    /// 指定されたコストの資源を支払えるかチェック
    /// </summary>
    /// <param name="cost">必要な資源（種類と枚数の辞書）</param>
    /// <returns>支払い可能ならtrue</returns>
    public bool CanAfford(Dictionary<ResourceType, int> cost)
    {
        return cost.All(kvp => Resources[kvp.Key] >= kvp.Value);
    }

    /// <summary>
    /// 指定されたコストの資源を支払う
    /// </summary>
    /// <param name="cost">支払う資源（種類と枚数の辞書）</param>
    public void SpendResources(Dictionary<ResourceType, int> cost)
    {
        foreach (var kvp in cost)
        {
            Resources[kvp.Key] -= kvp.Value;
        }
    }

    /// <summary>
    /// 資源カードを追加
    /// </summary>
    /// <param name="type">資源の種類</param>
    /// <param name="amount">追加する枚数（デフォルト: 1）</param>
    public void AddResource(ResourceType type, int amount = 1)
    {
        Resources[type] += amount;
    }
}
