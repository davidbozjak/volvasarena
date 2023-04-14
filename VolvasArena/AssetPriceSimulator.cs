record PriceChangeAlternative(double Odds, Func<double, double> ChangeFunc);

record AssetType(string Name);

record AssetPrice(AssetType Asset, int Tick, double Price);

class AssetPriceSimulator : IAssetPriceProvider
{
    private readonly IRandomProvider randomProvider;
    private readonly PriceChangeAlternative[] priceChangeAlternatives;


    private readonly List<AssetPrice> assetPrices = new();

    public AssetType AssetType { get; }

    private readonly Cached<int> cachedTicksSimulated;
    public int TicksSimulated => this.cachedTicksSimulated.Value;

    public double LatestAssetPriceValue => this.LatestAssetPrice.Price;

    private readonly Cached<AssetPrice> cachedAssetPrice;
    public AssetPrice LatestAssetPrice => this.cachedAssetPrice.Value;

    public IEnumerable<AssetPrice> AssetPrices => this.assetPrices.AsReadOnly();

    public AssetPriceSimulator(AssetType asset, double initialPrice, IRandomProvider randomProvider, IEnumerable<PriceChangeAlternative> priceChangeAlternatives)
    {
        this.AssetType = asset;
        this.assetPrices.Add(new AssetPrice(this.AssetType, 0, initialPrice));

        this.randomProvider = randomProvider;

        this.cachedTicksSimulated = new Cached<int>(() => this.assetPrices.Max(w => w.Tick));
        this.cachedAssetPrice = new Cached<AssetPrice>(() => this.assetPrices.Where(w => w.Tick == TicksSimulated).First());

#if DEBUG
        if (Math.Abs(priceChangeAlternatives.Sum(w => w.Odds) - 1) > 1e-3)
            throw new Exception("Expecting odds to sum up to 1");
#endif

        this.priceChangeAlternatives = priceChangeAlternatives.ToArray();
    }

    public void MakeTick()
    {
        var current = this.assetPrices.Last();

#if DEBUG
        if (current.Tick != this.assetPrices.Max(w => w.Tick))
            throw new Exception("Assuming current will always have largest tick");

        if (this.assetPrices.GroupBy(w => w.Tick).ToDictionary(w => w.Key, w => w.ToArray()).Any(w => w.Value.Length != 1))
            throw new Exception("Expecting ticks to be strictly increasing");
#endif

        this.ResetAllCachedValues();

        var chanceValue = this.randomProvider.NextDouble();
        var priceChangeAlternative = priceChangeAlternatives.Last();

        for (int i = 0; i < priceChangeAlternatives.Length - 1; i++)
        {
            var alternative = priceChangeAlternatives[i];

            if (chanceValue < alternative.Odds)
            {
                priceChangeAlternative = alternative;
                break;
            }

            chanceValue -= alternative.Odds;
        }

        assetPrices.Add(new AssetPrice(this.AssetType, current.Tick + 1, priceChangeAlternative.ChangeFunc(current.Price)));
    }

    private void ResetAllCachedValues()
    {
        this.cachedAssetPrice.Reset();
        this.cachedTicksSimulated.Reset();
    }
}