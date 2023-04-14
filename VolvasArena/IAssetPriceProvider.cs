interface IAssetPriceProvider
{
    IEnumerable<AssetPrice> AssetPrices { get; }
    
    AssetType AssetType { get; }
    
    AssetPrice LatestAssetPrice { get; }
    
    double LatestAssetPriceValue { get; }
    
    int TicksSimulated { get; }

    void MakeTick();
}