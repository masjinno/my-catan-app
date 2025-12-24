namespace CatanGame.Core.Models;

/// <summary>
/// 港の種類を表す列挙型
/// カタンには一般港（3:1）と専門港（2:1）の2種類がある
/// </summary>
public enum PortType
{
    /// <summary>一般港（3:1交換）- どの資源でも3:1レートで交換可能</summary>
    Generic,

    /// <summary>木材専門港（2:1交換）- 木材を2:1レートで交換可能</summary>
    Wood,

    /// <summary>土（レンガ）専門港（2:1交換）- レンガを2:1レートで交換可能</summary>
    Brick,

    /// <summary>羊専門港（2:1交換）- 羊を2:1レートで交換可能</summary>
    Sheep,

    /// <summary>小麦専門港（2:1交換）- 小麦を2:1レートで交換可能</summary>
    Wheat,

    /// <summary>鉄（鉱石）専門港（2:1交換）- 鉱石を2:1レートで交換可能</summary>
    Ore
}
