using System.Text.RegularExpressions;

class AssetPriceSimpleCSVReader : IAssetPriceProvider
{
    public IEnumerable<AssetPrice> AssetPrices => this.cachedPrices.Value.Take(TicksSimulated);

    public AssetType AssetType { get; }

    public AssetPrice LatestAssetPrice => this.cachedPrices.Value[this.TicksSimulated - 1];

    public double LatestAssetPriceValue => this.LatestAssetPrice.Price;

    public int TicksSimulated { get; private set; }

    public int TotalTicksAvaliable => this.cachedPrices.Value.Count;

    private readonly Cached<List<AssetPrice>> cachedPrices;

    private readonly string sourceFilePath;

    public AssetPriceSimpleCSVReader(AssetType assetType, string filePath)
    {
        this.sourceFilePath = filePath;
        this.AssetType = assetType;
        this.cachedPrices = new Cached<List<AssetPrice>>(ReadPricesFromCSV);
    }

    public void MakeTick()
    {
        this.TicksSimulated++;
    }

    private List<AssetPrice> ReadPricesFromCSV()
    {
        using var stream = new StreamReader(this.sourceFilePath);

        var data = new List<AssetPrice>();

        Regex numRegex = new(@"\d+,\d\d");

        int tick = 1;

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();

            var numbers = numRegex.Matches(line).Select(w => double.Parse(w.Value));

            foreach (var number in numbers)
            {
                data.Add(new AssetPrice(this.AssetType, tick++, number));
            }
        }

        return data;
    }
}