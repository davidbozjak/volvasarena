record CompletedTransaction(Asset Asset, AssetPrice PurchasePrice, AssetPrice SellPrice);

class TraderBot : IDisposable
{
    public delegate IEnumerable<(int amount, double price)> GetAmountToBuyDelegate(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator);
    public delegate IEnumerable<(double price, IEnumerable<Asset> assets)> GetAssetsToSellDelegate(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator);

    public AssetType TradedAssetType { get; }
    
    private readonly List<Asset> currentlyOwnedAssets = new();
    private readonly List<MarketplaceBuyOrder> outstandingBuyOrders = new();
    private readonly List<MarketplaceSellOrder> outstandingSellOrders = new();
    private readonly List<CompletedTransaction> completedTransactions = new();

    private readonly GetAmountToBuyDelegate getAmountToBuy;
    private readonly GetAssetsToSellDelegate getAssetsToSell;

    public double CurrentMoney { get; private set; }

    private double lastObservedPrice;

    public double CurrentTotalAssets => this.CurrentMoney +
        this.outstandingBuyOrders.Sum(w => w.ReservedAssets) +
        this.currentlyOwnedAssets.Count * lastObservedPrice +
        this.outstandingSellOrders.Sum(w => w.AssetsToSell.Count() * lastObservedPrice);
    
    public double TotalRealizedProfit { get; private set; }

    public double TotalTransactionCost { get; private set; }

    public IEnumerable<CompletedTransaction> CompletedTransactions => this.completedTransactions.AsReadOnly();

    public string Name { get; }

    public TraderBot(
        string name,
        double startMoney,
        AssetType assetType,
        GetAmountToBuyDelegate getAmountToBuy,
        GetAssetsToSellDelegate getAssetsToSell)
    {
        this.Name = name;
        this.CurrentMoney = startMoney;
        this.TradedAssetType = assetType;
        this.getAmountToBuy = getAmountToBuy;
        this.getAssetsToSell = getAssetsToSell;
    }

    public IEnumerable<MarketplaceOrder> SubmitOrders(int tick, AssetPrice lastPrice, IEnumerable<AssetPrice> historicalPrices, ITransactionCostCalculator transactionCostCalculator)
    {
        this.lastObservedPrice = lastPrice.Price;

        IEnumerable<(double priceToSell, IEnumerable<Asset> assetsToOffer)> submittedSellOffers = getAssetsToSell(tick, this.CurrentMoney, this.lastObservedPrice, historicalPrices, this.currentlyOwnedAssets.AsReadOnly(), this.outstandingBuyOrders.AsReadOnly(), this.outstandingSellOrders.AsReadOnly(), transactionCostCalculator);

        foreach (var sellOffer in submittedSellOffers)
        {
            if (sellOffer.assetsToOffer.Any())
            {
                var sellOrder = new MarketplaceSellOrder(sellOffer.assetsToOffer, 10, sellOffer.priceToSell);
                this.outstandingSellOrders.Add(sellOrder);

                this.currentlyOwnedAssets.RemoveAll(w => sellOffer.assetsToOffer.Contains(w));

                sellOrder.OrderFulfilledEvent += OnOrderFulfilled;
                sellOrder.OrderCancelledEvent += OnOrderCancelled;

                yield return sellOrder;
            }
        }

        IEnumerable<(int amountToBuy, double priceToBuyAt)> submittedBuyOffers = getAmountToBuy(tick, this.CurrentMoney, this.lastObservedPrice, historicalPrices, this.currentlyOwnedAssets.AsReadOnly(), this.outstandingBuyOrders.AsReadOnly(), this.outstandingSellOrders.AsReadOnly(), transactionCostCalculator);
        
        foreach (var buyOffer in submittedBuyOffers)
        {
            if (buyOffer.amountToBuy > 0)
            {
                var buyOrder = new MarketplaceBuyOrder(this.TradedAssetType, 10, buyOffer.priceToBuyAt, buyOffer.amountToBuy);
                this.CurrentMoney -= buyOrder.ReservedAssets;

                if (this.CurrentMoney < 0)
                    throw new Exception();

                this.outstandingBuyOrders.Add(buyOrder);
                buyOrder.OrderFulfilledEvent += OnOrderFulfilled;
                buyOrder.OrderCancelledEvent += OnOrderCancelled;

                yield return buyOrder;
            }
        }
    }

    private void OnOrderFulfilled(object sender, FulfilledOrderReceipt receipt)
    {
        if (receipt.Order is MarketplaceBuyOrder buyOrder)
        {
            this.currentlyOwnedAssets.AddRange(receipt.TransferredAssets);
            this.outstandingBuyOrders.Remove(buyOrder);
        }
        else if (receipt.Order is MarketplaceSellOrder sellOrder)
        {
            this.currentlyOwnedAssets.RemoveAll(w => receipt.TransferredAssets.Contains(w));
            this.TotalRealizedProfit += receipt.TransferredAssets.Sum(w => receipt.FinalPrice.Price - w.BoughtAtPrice.Price) - receipt.Courtage;
            this.outstandingSellOrders.Remove(sellOrder);

            foreach (var asset in sellOrder.AssetsToSell)
            {
                this.completedTransactions.Add(new CompletedTransaction(asset, asset.BoughtAtPrice, receipt.FinalPrice));
            }
        }
        else throw new Exception();

        this.CurrentMoney += receipt.ReturnedAssets;
        this.TotalTransactionCost += receipt.Courtage;

        ((MarketplaceOrder)sender).OrderCancelledEvent -= OnOrderCancelled;
        ((MarketplaceOrder)sender).OrderFulfilledEvent -= OnOrderFulfilled;
    }

    private void OnOrderCancelled(object sender, CancelledOrderReceipt receipt)
    {
        if (receipt.Order is MarketplaceBuyOrder buyOrder)
        {
            this.outstandingBuyOrders.Remove(buyOrder);
        }
        else if (receipt.Order is MarketplaceSellOrder sellOrder)
        {
            this.outstandingSellOrders.Remove(sellOrder);

            this.currentlyOwnedAssets.AddRange(sellOrder.AssetsToSell);

#if DEBUG
            var assetDict = this.currentlyOwnedAssets
                .GroupBy(w => w)
                .ToDictionary(w => w, w => w.ToList());
            if (assetDict.Values.Any(w => w.Count > 1))
                throw new Exception("Each asset may only be on offer once");
#endif
        }
        else throw new Exception();

        this.CurrentMoney += receipt.ReturnedAssets;

        ((MarketplaceOrder)sender).OrderCancelledEvent -= OnOrderCancelled;
        ((MarketplaceOrder)sender).OrderFulfilledEvent -= OnOrderFulfilled;
    }

    public void Dispose()
    {
        foreach (var order in this.outstandingBuyOrders)
        {
            order.OrderCancelledEvent -= OnOrderCancelled;
            order.OrderFulfilledEvent -= OnOrderFulfilled;
        }

        this.outstandingBuyOrders.Clear();

        foreach (var order in this.outstandingSellOrders)
        {
            order.OrderCancelledEvent -= OnOrderCancelled;
            order.OrderFulfilledEvent -= OnOrderFulfilled;
        }

        this.outstandingSellOrders.Clear();
    }
}