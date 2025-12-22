namespace CatanGame.Core.Models;

public class HexTile
{
    public int Q { get; set; }
    public int R { get; set; }
    public ResourceType ResourceType { get; set; }
    public int? NumberToken { get; set; }
    public bool HasRobber { get; set; }

    public HexTile(int q, int r, ResourceType resourceType, int? numberToken = null)
    {
        Q = q;
        R = r;
        ResourceType = resourceType;
        NumberToken = numberToken;
        HasRobber = resourceType == ResourceType.Desert;
    }
}
