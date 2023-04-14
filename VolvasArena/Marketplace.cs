class Marketplace
{
    private readonly AssetFactory assetFactory = new();

    public IAssetPriceProvider AssetPriceProvider { get; }

    public ITransactionCostCalculator TransactionCostCalculator { get; }

    private readonly List<TraderBot> subscribedTraders = new();

    private readonly List<MarketplaceBuyOrder> ongoingBuyOrders = new();

    private readonly List<MarketplaceSellOrder> ongoingSellOrders = new();

    public const int TicksOfHistoryToProvide = 10;

    public Marketplace(IAssetPriceProvider assetPriceProvider, ITransactionCostCalculator transactionCostCalculator)
    {
        AssetPriceProvider = assetPriceProvider;
        TransactionCostCalculator = transactionCostCalculator;
    }

    public void Subscribe(TraderBot traderBot)
    {
        this.subscribedTraders.Add(traderBot);
    }

    public void SubscribeRange(IEnumerable<TraderBot> collection)
    {
        this.subscribedTraders.AddRange(collection);
    }

    public void RunForInitialPeriod()
    {
        if (this.AssetPriceProvider.TicksSimulated != 0)
            throw new Exception("Expecting this method will only be run at the very beginning, to generate first historical data");

        for (int i = 0; i < TicksOfHistoryToProvide; i++)
            this.AssetPriceProvider.MakeTick();
    }

    public void MakeTick()
    {
        this.AssetPriceProvider.MakeTick();

        this.HandleExistingOrders();

        if (!subscribedTraders.Any())
        {
            return;
        }

        var last10Prices = this.AssetPriceProvider.AssetPrices.OrderByDescending(w => w.Tick).Take(TicksOfHistoryToProvide).ToList();
        var lastPrice = last10Prices[0];

        foreach (var trader in subscribedTraders)
        {
            var newOrders = trader.SubmitOrders(this.AssetPriceProvider.TicksSimulated, lastPrice, last10Prices, this.TransactionCostCalculator);

            foreach (var order in newOrders)
            {
                if (order.TicksToLive <= 0)
                    throw new Exception();

                if (order is MarketplaceBuyOrder buyOrder)
                {
                    this.ongoingBuyOrders.Add(buyOrder);
                }
                else if (order is MarketplaceSellOrder sellOrder)
                {
                    this.ongoingSellOrders.Add(sellOrder);

#if DEBUG
                    var assetDict = this.ongoingSellOrders.Where(w => !w.IsCancelled).SelectMany(w => w.AssetsToSell)
                        .GroupBy(w => w)
                        .ToDictionary(w => w, w => w.ToList());
                    if (assetDict.Values.Any(w => w.Count > 1))
                        throw new Exception("Each asset may only be on offer once");
#endif
                }
            }
        }
    }

    private void HandleExistingOrders()
    {
        foreach (var buyOrder in this.ongoingBuyOrders)
        {
            buyOrder.HandleTick();
        }

        foreach (var sellOrder in this.ongoingSellOrders)
        {
            sellOrder.HandleTick();
        }

        this.ongoingBuyOrders.RemoveAll(w => w.IsCancelled);
        this.ongoingSellOrders.RemoveAll(w => w.IsCancelled);

        foreach (var buyOrder in this.ongoingBuyOrders)
        {
            if (this.AssetPriceProvider.LatestAssetPriceValue <= buyOrder.Price)
            {
                var courtage = this.TransactionCostCalculator.TransactionCostToBuy(this.AssetPriceProvider.LatestAssetPrice, buyOrder.Amount);

                buyOrder.FulfillOrder(this.assetFactory, this.AssetPriceProvider.LatestAssetPrice, courtage);
            }
        }

        foreach (var sellOrder in this.ongoingSellOrders)
        {
            if (this.AssetPriceProvider.LatestAssetPriceValue >= sellOrder.Price)
            {
                var courtage = this.TransactionCostCalculator.TransactionCostToSell(sellOrder.AssetsToSell);

                sellOrder.FulfillOrder(this.assetFactory, this.AssetPriceProvider.LatestAssetPrice, courtage);
            }
        }

        this.ongoingBuyOrders.RemoveAll(w => w.IsFulfilled);
        this.ongoingSellOrders.RemoveAll(w => w.IsFulfilled);
    }
}

abstract class MarketplaceOrder
{
    public AssetType AssetType { get; }

    public int TicksToLive { get; private set; }

    public double Price { get; }

    public int Amount { get; private set; }

    public double ReservedAssets { get; }

    public bool IsCancelled { get; private set; } = false;

    public bool IsFulfilled { get; private set; } = false;


    public event EventHandler<FulfilledOrderReceipt> OrderFulfilledEvent = delegate { };


    public event EventHandler<CancelledOrderReceipt> OrderCancelledEvent = delegate { };

    public MarketplaceOrder(AssetType assetType, int ticksToLive, double price, int amount, double reservedAssets)
    {
        if (ticksToLive <= 0)
            throw new ArgumentException();

        if (price <= 0)
            throw new ArgumentException();

        if (amount <= 0)
            throw new ArgumentException();

        if (reservedAssets < 0)
            throw new ArgumentException();

        AssetType = assetType;
        TicksToLive = ticksToLive;
        Price = price;
        Amount = amount;
        this.ReservedAssets = reservedAssets;
    }   

    public void CancelOrder()
    {
        this.IsCancelled = true;

        this.OrderCancelledEvent(this, new CancelledOrderReceipt(this, this.ReservedAssets));
    }

    public void FulfillOrder(AssetFactory assetFactory, AssetPrice assetPrice, double courtage)
    {
        this.IsFulfilled = true;

        this.OrderFulfilledEvent(this, RaiseFulfilledEvent(assetFactory, assetPrice, courtage));
    }

    public void HandleTick()
    {
        if (this.IsFulfilled || this.IsCancelled)
            return;

        this.TicksToLive--;

        if (this.TicksToLive < 0)
            throw new Exception("Expecting this to be cleaned up before this happens");

        if (this.TicksToLive == 0)
            this.CancelOrder();
    }

    protected abstract FulfilledOrderReceipt RaiseFulfilledEvent(AssetFactory assetFactory, AssetPrice assetPrice, double courtage);
}

class MarketplaceBuyOrder : MarketplaceOrder
{
    public MarketplaceBuyOrder(AssetType assetType, int ticksToLive, double price, int amount)
        : base(assetType, ticksToLive, price, amount, price * amount)
    {
    }

    protected override FulfilledOrderReceipt RaiseFulfilledEvent(AssetFactory assetFactory, AssetPrice assetPrice, double courtage)
    {
        if (assetPrice.Price * this.Amount > this.ReservedAssets)
            throw new Exception();

        var assets = Enumerable.Range(0, Amount).Select(w => assetFactory.GetUniqueAsset(this.AssetType, assetPrice));

#if DEBUG
        var assetDict = assets
            .GroupBy(w => w)
            .ToDictionary(w => w, w => w.ToList());
                
        if (assetDict.Values.Any(w => w.Count > 1))
            throw new Exception("Each asset may only be on offer once");
#endif

        var finalPrice = Amount * assetPrice.Price + courtage;

        var returnedAssets = this.ReservedAssets - finalPrice;

        return new FulfilledOrderReceipt(this, assetPrice, returnedAssets, courtage, assets);
    }
}

class MarketplaceSellOrder : MarketplaceOrder
{
    private readonly List<Asset> assetsToSell;

    public IEnumerable<Asset> AssetsToSell => this.assetsToSell.AsReadOnly();

    public MarketplaceSellOrder(IEnumerable<Asset> assetsToSell, int ticksToLive, double price)
        : base(assetsToSell.First().Type, ticksToLive, price, assetsToSell.Count(), 0)
    {
        this.assetsToSell = assetsToSell.ToList();
    }

    protected override FulfilledOrderReceipt RaiseFulfilledEvent(AssetFactory assetFactory, AssetPrice assetPrice, double courtage)
    {
        var finalPrice = Amount * assetPrice.Price;

        return new FulfilledOrderReceipt(this, assetPrice, finalPrice - courtage, courtage, assetsToSell);
    }
}

record FulfilledOrderReceipt(MarketplaceOrder Order, AssetPrice FinalPrice, double ReturnedAssets, double Courtage, IEnumerable<Asset> TransferredAssets); 

record CancelledOrderReceipt(MarketplaceOrder Order, double ReturnedAssets);