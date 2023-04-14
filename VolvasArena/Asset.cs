record Asset(int Id, AssetType Type, AssetPrice BoughtAtPrice);

class AssetFactory
{
    private readonly object lockingObject = new object();
    private int numCreated = 0;

    public Asset GetUniqueAsset(AssetType type, AssetPrice boughtAtPrice)
    {
        lock (lockingObject)
        {
            return new Asset(numCreated++, type, boughtAtPrice);
        }
    }
}