# Catan Game 設計資料

## プロジェクト概要

カタンボードゲームのWindowsデスクトップアプリケーション

### 技術スタック
- **言語**: C#
- **フレームワーク**: WPF (.NET 8.0)
- **設計パターン**: MVVM

### 基本仕様
- 4人プレイの基本ルール
- 拡張版なし
- 将来的にAI対戦実装予定
- オンライン対戦不要
- コマンド実行はユーザー側で実施

---

## アーキテクチャ

### プロジェクト構成

```
my-catan-app/
├── .gitignore
├── CONVERSATION_LOG.md      # 対話ログ
├── DESIGN_DOCUMENT.md       # この設計資料
└── src/
    └── CatanGame/
        ├── CatanGame.sln
        ├── CatanGame.Core/      # Model層（ゲームロジック）
        └── CatanGame.App/       # View/ViewModel層（WPF UI）
```

### MVVM設計

#### Model層 (CatanGame.Core)
- **責務**: ゲームルール、状態管理、ビジネスロジック
- **特徴**: UI非依存、テスト可能、再利用可能
- **内容**: エンティティクラス、ゲームロジック

#### ViewModel層 (CatanGame.App/ViewModels)
- **責務**: UI用のデータ変換、コマンド実装、状態管理
- **特徴**: INotifyPropertyChanged実装、データバインディング対応
- **内容**: ViewModelBase、各画面のViewModel

#### View層 (CatanGame.App/Views)
- **責務**: 画面表示のみ
- **特徴**: XAMLで宣言的に記述、ロジックなし
- **内容**: Window、UserControl、DataTemplate

---

## データモデル

### 座標系

**キューブ座標系（Cube Coordinates）**
- Q軸、R軸の2つで六角形の位置を表現
- 隣接タイルの計算が簡単
- 頂点・辺もDirection（0-5）で表現

```
    0
 5     1
    H
 4     2
    3
```

### コアエンティティ

#### ResourceType (列挙型)
資源の種類
- Wood (木材)
- Brick (レンガ)
- Sheep (羊)
- Wheat (麦)
- Ore (鉱石)
- Desert (砂漠)

#### HexTile
六角形タイル
- Q, R: 座標
- ResourceType: 資源タイプ
- NumberToken: 数字トークン（2-12、砂漠はnull）
- HasRobber: 盗賊がいるか

#### PlayerColor (列挙型)
プレイヤーの色
- Red
- Blue
- White
- Orange

#### Player
プレイヤー
- Name: プレイヤー名
- Color: プレイヤー色
- Resources: 資源カードの所持数（Dictionary<ResourceType, int>）
- VictoryPoints: 勝利点
- SettlementCount: 残りの開拓地数（初期5）
- CityCount: 残りの都市数（初期4）
- RoadCount: 残りの道路数（初期15）
- DevelopmentCards: 発展カード

#### VertexPosition
頂点位置（開拓地・都市の配置位置）
- Q, R: タイル座標
- Direction: 方向（0-5）

#### EdgePosition
辺位置（道路の配置位置）
- Q, R: タイル座標
- Direction: 方向（0-5）

#### Settlement
開拓地/都市
- Position: 頂点位置
- Owner: 所有プレイヤー
- IsCity: 都市かどうか

#### Road
道路
- Position: 辺位置
- Owner: 所有プレイヤー

#### Board
ゲームボード
- Tiles: 全タイルのリスト（19枚）
- Settlements: 配置された開拓地/都市（Dictionary）
- Roads: 配置された道路（Dictionary）

**主要メソッド:**
- `CanPlaceSettlement()`: 開拓地を配置可能か判定
- `CanPlaceRoad()`: 道路を配置可能か判定
- `PlaceSettlement()`: 開拓地を配置
- `PlaceRoad()`: 道路を配置
- `UpgradeToCity()`: 都市にアップグレード
- `GetSettlementsOnTile()`: タイルに隣接する開拓地を取得

#### DevelopmentCard
発展カード
- Type: カードタイプ（Knight, VictoryPoint, RoadBuilding, YearOfPlenty, Monopoly）
- HasBeenPlayed: 使用済みか

#### GameState
ゲーム全体の状態
- Board: ゲームボード
- Players: プレイヤーリスト（4人）
- CurrentPlayerIndex: 現在のプレイヤー番号
- Phase: ゲームフェーズ（Setup, Playing, Ended）
- DiceRoll: サイコロの出目

**主要メソッド:**
- `NextTurn()`: 次のプレイヤーへ
- `RollDice()`: サイコロを振る
- `DistributeResources()`: 資源を配布
- `CheckWinner()`: 勝者判定

---

## UI設計

### 座標変換

六角形の中心座標計算（キューブ座標→ピクセル座標）:
```csharp
double x = size * (3.0 / 2.0 * q);
double y = size * (Math.Sqrt(3) / 2.0 * q + Math.Sqrt(3) * r);
```

### 色設定

資源タイプごとの表示色:
- Wood (木材): `#228B22` (緑)
- Brick (レンガ): `#8B4513` (茶色)
- Sheep (羊): `#90EE90` (薄緑)
- Wheat (麦): `#FFD700` (金色)
- Ore (鉱石): `#808080` (灰色)
- Desert (砂漠): `#F4A460` (サンディブラウン)

### コンポーネント

#### MainWindow
メインウィンドウ
- 左側: ボード表示エリア（ScrollViewer + Canvas）
- 右側: コントロールパネル（プレイヤー情報、コマンドボタン）

#### HexagonControl
六角形タイルのUserControl
- Polygonで六角形描画
- 中央に数字トークン表示（白い円）

---

## ゲームルール

### ボード構成
- タイル数: 19枚
- 資源配分:
  - 木材: 4枚
  - レンガ: 3枚
  - 羊: 4枚
  - 麦: 4枚
  - 鉱石: 3枚
  - 砂漠: 1枚
- 数字トークン: 2-12（7除く）
  - 各数字2つずつ（ただし2と12は1つのみ）

### 建設物の上限
- 開拓地: 各プレイヤー5個
- 都市: 各プレイヤー4個
- 道路: 各プレイヤー15本

### 建設コスト
- **道路**: 木材×1、レンガ×1
- **開拓地**: 木材×1、レンガ×1、羊×1、麦×1
- **都市**: 麦×2、鉱石×3
- **発展カード**: 羊×1、麦×1、鉱石×1

### 勝利条件
- 10勝利点に到達したプレイヤーの勝利

### 勝利点の獲得方法
- 開拓地: 1点
- 都市: 2点
- 最長交易路: 2点
- 最大騎士力: 2点
- 勝利点カード: 1点

---

## 開発フェーズ

### フェーズ1: プロジェクト構造とボード表示 ✅
- ソリューション・プロジェクト作成
- MVVMインフラ構築
- コアモデル実装
- 六角形タイルのボード表示
- 基本的なゲーム状態管理

### フェーズ2: 初期配置（予定）
- クリック可能な頂点（開拓地配置用）
- クリック可能な辺（道路配置用）
- 初期配置ルール実装（各プレイヤー2つずつ）
- 初期資源配布
- 開拓地・道路の可視化

### フェーズ3: 基本ゲームループ（予定）
- サイコロ振りの改善
- 資源配布のUI表示
- ターン進行管理
- プレイヤーの手札表示

### フェーズ4: 建設と交易（予定）
- 通常フェーズでの建設
- プレイヤー間の交易
- 発展カードシステム
- 盗賊の移動

### フェーズ5: 勝利条件とAI（予定）
- 最長交易路・最大騎士力
- 勝利点の正確な計算
- AI実装
- ゲーム終了処理

---

## 技術的な実装詳細

### データバインディング

XAMLでのConverter使用例:
```xaml
<Window.Resources>
    <converters:StringToVisibilityConverter x:Key="StringToVisibilityConverter"/>
</Window.Resources>
```

### コマンドパターン

RelayCommandによるコマンド実装:
```csharp
public ICommand RollDiceCommand { get; }

RollDiceCommand = new RelayCommand(_ => RollDice(), _ => CanRollDice());
```

### 配置ルール検証

**開拓地の配置ルール:**
1. 既に開拓地が配置されていない
2. 隣接する頂点に開拓地が配置されていない（距離ルール）
3. 初期配置でない場合、自分の道路が隣接している

**道路の配置ルール:**
1. 既に道路が配置されていない
2. 自分の開拓地が隣接している、または自分の道路が隣接している

---

## 将来のAI実装に向けて

### 設計上の考慮点
- ゲームロジックをCoreに完全分離
- Playerクラスを継承してAIPlayerクラスを作成可能に
- 配置ルール検証メソッドを独立実装
- ゲーム状態の読み取りが容易

### AI実装のポイント
- 評価関数の設計（資源、位置、勝利点）
- モンテカルロ木探索の適用検討
- 交易の戦略パターン
- 複数の難易度設定

---

## ビルド・実行

### 必要環境
- .NET 8.0 SDK
- Windows OS

### ビルドコマンド
```bash
cd src\CatanGame
dotnet build
```

### 実行コマンド
```bash
dotnet run --project CatanGame.App
```

---

## 参考資料

### カタン公式ルール
- 基本セット
- 標準的な4人プレイ

### 技術参考
- WPF公式ドキュメント
- MVVM設計パターン
- 六角形グリッドアルゴリズム（Cube Coordinates）
