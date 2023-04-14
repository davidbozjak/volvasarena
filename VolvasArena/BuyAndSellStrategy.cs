static class BuyAndSellStrategy
{
    public static IRandomProvider? RandomProvider;

    public static IEnumerable<(string, TraderBot.GetAmountToBuyDelegate)> GetBuySrategies()
    {
        var methods = typeof(BuyStrategies)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        foreach (var method in methods)
        {
            var customAttributes = method.GetCustomAttributes(typeof(SkipWhenFormingExecutionListAttribute), false);

            bool shouldRun = customAttributes == null ||
                customAttributes.Length == 0 ||
                ((SkipWhenFormingExecutionListAttribute)customAttributes[0]).Skip != true;

            if (shouldRun)
            {
                yield return (method.Name, method.CreateDelegate<TraderBot.GetAmountToBuyDelegate>());
            }
        }
    }

    public static IEnumerable<(string, TraderBot.GetAssetsToSellDelegate)> GetSellSrategies()
    {
        var methods = typeof(SellStrategies)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        foreach (var method in methods)
        {
            var customAttributes = method.GetCustomAttributes(typeof(SkipWhenFormingExecutionListAttribute), false);

            bool shouldRun = customAttributes == null ||
                customAttributes.Length == 0 ||
                ((SkipWhenFormingExecutionListAttribute)customAttributes[0]).Skip != true;

            if (shouldRun)
            {
                yield return (method.Name, method.CreateDelegate<TraderBot.GetAssetsToSellDelegate>());
            }
        }
    }

    public static class BuyStrategies
    {
        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> BuyRandomAmountAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            int maxAfordable = (int)Math.Floor(currentMoneyAvaliable / lastPrice);

            var amount = RandomProvider.Next(maxAfordable);

            yield return (amount, lastPrice);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndBuyAsManyAsYouCanAffordAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            return LeaveOldOrBuyAsManyAsYouCanAffordAtLastPrice(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, Enumerable.Empty<MarketplaceBuyOrder>(), outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndBuyHalfAsManyAsYouCanAffordAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            return LeaveOldOrBuyHalfAsManyAsYouCanAffordAtLastPrice(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, Enumerable.Empty<MarketplaceBuyOrder>(), outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndBuyAsManyAsYouCanAffordAtTwoPercentLess(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            return LeaveOldOrBuyAsManyAsYouCanAffordAtTwoPercentLess(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, Enumerable.Empty<MarketplaceBuyOrder>(), outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndBuyHalfAsManyAsYouCanAffordAtTwoPercentLess(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            return LeaveOldOrBuyHalfAsManyAsYouCanAffordAtTwoPercentLess(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, Enumerable.Empty<MarketplaceBuyOrder>(), outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndBuyTheDipAt99(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            return LeaveOldOrBuyTheDipAt99(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, Enumerable.Empty<MarketplaceBuyOrder>(), outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndBuyTheDipAtLatest(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            return LeaveOldOrBuyTheDipAtLatest(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, Enumerable.Empty<MarketplaceBuyOrder>(), outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndBuyTheDipAt101(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            return LeaveOldOrBuyTheDipAt101(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, Enumerable.Empty<MarketplaceBuyOrder>(), outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> CancelOldAndSubmitSpreadTwoPercentLessAndUpwards(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            currentMoneyAvaliable = CancelOutstandingBuyOrdersAndGetTotalMoney(currentMoneyAvaliable, outsdandingBuyOrders);

            int numberOfBuckets = 6;

            var orderPrice = lastPrice * 0.97;
            var maxPrice = lastPrice * 1.04;
            var step = (maxPrice - orderPrice) / numberOfBuckets;

            var approxNumberOfAffordable = currentMoneyAvaliable / maxPrice;

            int amountPerBucket = (int)(approxNumberOfAffordable / numberOfBuckets);

            for (int i = 0; i < numberOfBuckets; i++)
            {
                yield return (amountPerBucket, orderPrice);
                currentMoneyAvaliable -= amountPerBucket * orderPrice;

                orderPrice += step;
            }

            yield return ((int)(currentMoneyAvaliable / orderPrice), orderPrice);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> LeaveOldOrBuyAsManyAsYouCanAffordAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrBuyAsManyAsYouCanAffordAtX(1, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> LeaveOldOrBuyHalfAsManyAsYouCanAffordAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrBuyAsManyAsYouCanAffordAtX(1, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> LeaveOldOrBuyAsManyAsYouCanAffordAtTwoPercentLess(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrBuyAsManyAsYouCanAffordAtX(0.98, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        private static IEnumerable<(int amount, double price)> LeaveOldOrBuyAsManyAsYouCanAffordAtX(double factor, int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            if (outsdandingBuyOrders.Any())
            {
                yield return (0, -1);
            }
            else
            {
                var thresholdPrice = lastPrice * factor;
                int maxAfordable = (int)Math.Floor(currentMoneyAvaliable / thresholdPrice);

                yield return (maxAfordable, thresholdPrice);
            }
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> LeaveOldOrBuyHalfAsManyAsYouCanAffordAtTwoPercentLess(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrBuyHalfAsManyAsYouCanAffordAtX(0.98, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        private static IEnumerable<(int amount, double price)> LeaveOldOrBuyHalfAsManyAsYouCanAffordAtX(double factor, int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            if (outsdandingBuyOrders.Any())
            {
                yield return (0, -1);
            }
            else
            {
                var thresholdPrice = lastPrice * factor;
                int maxAfordable = (int)Math.Floor(currentMoneyAvaliable / thresholdPrice);

                yield return (maxAfordable / 2, thresholdPrice);
            }
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> LeaveOldOrBuyTheDipAt99(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrBuyTheDipAtX(0.99, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> LeaveOldOrBuyTheDipAtLatest(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrBuyTheDipAtX(1, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        ////[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(int amount, double price)> LeaveOldOrBuyTheDipAt101(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrBuyTheDipAtX(1.01, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        private static IEnumerable<(int amount, double price)> LeaveOldOrBuyTheDipAtX(double factor, int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            if (outsdandingBuyOrders.Any())
            {
                yield return (0, -1);
            }
            else
            {
                int numberOfFallingDays = 0;
                var prices = latest10Prices.ToList();

                for (int i = 1; i < prices.Count; i++)
                {
                    if (prices[i].Price < prices[i - 1].Price)
                        break;

                    numberOfFallingDays++;
                }

                var thresholdPrice = lastPrice * factor;
                int maxAfordable = (int)Math.Floor(currentMoneyAvaliable / thresholdPrice);

                //number of falling days is acting as a weight, with all 10 days being falling (max) we will buy 100% of what we can afford
                int numToBuy = numberOfFallingDays * maxAfordable / 10;

                yield return (numToBuy, thresholdPrice);
            }
        }

    }

    public static class SellStrategies
    {
        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> SellRandomAmountAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var numToSell = RandomProvider.Next(currentOwnedAssets.Count());

            yield return (lastPrice, currentOwnedAssets.OrderBy(w => w.BoughtAtPrice.Price).Take(numToSell));
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> SellRandomAmountOfProfitableAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var numToSell = RandomProvider.Next(currentOwnedAssets.Where(w => w.BoughtAtPrice.Price < lastPrice).Count());

            yield return (lastPrice, currentOwnedAssets.OrderBy(w => w.BoughtAtPrice.Price).Take(numToSell));
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellHalfProfitableAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrSellHalfProfitableAtX(1, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellHalfProfitableAtTwoPercentMore(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrSellHalfProfitableAtX(1.02, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        private static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellHalfProfitableAtX(double factor, int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            if (outsdandingSellOrders.Any())
            {
                yield return (0, Enumerable.Empty<Asset>());
            }
            else
            {
                var thresholdPrice = lastPrice * 1.02;
                var currentAmount = currentOwnedAssets.Count();

                yield return (thresholdPrice, currentOwnedAssets
                    .Where(w => w.BoughtAtPrice.Price < thresholdPrice)
                    .OrderBy(w => w.BoughtAtPrice.Price)
                    .Take(currentAmount / 2));
            }
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellAllProfitableAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrSellAllProfitableAtX(1, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellAllProfitableAtTwoPercentMore(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrSellAllProfitableAtX(1.02, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        private static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellAllProfitableAtX(double factor, int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            if (outsdandingSellOrders.Any())
            {
                yield return (0, Enumerable.Empty<Asset>());
            }
            else
            {
                var thresholdPrice = lastPrice * factor;
                var currentAmount = currentOwnedAssets.Count();

                yield return (thresholdPrice, currentOwnedAssets
                    .Where(w => w.BoughtAtPrice.Price < thresholdPrice));
            }
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellProfitablePeakAt99(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrSellProfitablePeakAtX(0.99, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellProfitablePeakAtLatest(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrSellProfitablePeakAtX(1, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellProfitablePeakAt101(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            return LeaveOldOrSellProfitablePeakAtX(1.01, currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, currentOwnedAssets, outsdandingBuyOrders, outsdandingSellOrders, transactionCostCalculator);
        }

        private static IEnumerable<(double price, IEnumerable<Asset> assets)> LeaveOldOrSellProfitablePeakAtX(double factor, int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            if (outsdandingSellOrders.Any())
            {
                yield return (0, Enumerable.Empty<Asset>());
            }
            else
            {
                int numberOfRisingDays = 0;
                var prices = latest10Prices.ToList();

                for (int i = 1; i < prices.Count; i++)
                {
                    if (prices[i].Price > prices[i - 1].Price)
                        break;

                    numberOfRisingDays++;
                }

                var orderPrice = lastPrice * factor;

                var assetsToSell = currentOwnedAssets
                    .Where(w => w.BoughtAtPrice.Price < orderPrice)
                    .OrderBy(w => w.BoughtAtPrice.Price);

                if (assetsToSell.Any())
                {
                    var maxAvaliableToSell = assetsToSell.Count();

                    //number of falling days is acting as a weight, with all 10 days being falling (max) we will buy 100% of what we can afford
                    int maxNumToSell = numberOfRisingDays * maxAvaliableToSell / 10;

                    yield return (orderPrice, assetsToSell.Take(maxNumToSell));
                }
            }
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellAllProfitableAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            return LeaveOldOrSellAllProfitableAtLastPrice(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, allAssets, outsdandingBuyOrders, Enumerable.Empty<MarketplaceSellOrder>(), transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellHalfProfitableAtLastPrice(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            return LeaveOldOrSellHalfProfitableAtLastPrice(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, allAssets, outsdandingBuyOrders, Enumerable.Empty<MarketplaceSellOrder>(), transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellHalfProfitableAtTwoPercentMore(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            return LeaveOldOrSellHalfProfitableAtTwoPercentMore(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, allAssets, outsdandingBuyOrders, Enumerable.Empty<MarketplaceSellOrder>(), transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellAllProfitableAtTwoPercentMore(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            return LeaveOldOrSellAllProfitableAtTwoPercentMore(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, allAssets, outsdandingBuyOrders, Enumerable.Empty<MarketplaceSellOrder>(), transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellSpreadOfProfitable(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssetsList = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            if (!allAssetsList.Any())
            {
                yield return (-1, Enumerable.Empty<Asset>());
            }
            else
            {
                var minBoughtPrice = allAssetsList.Min(w => w.BoughtAtPrice.Price);

                var orderPrice = Math.Max(lastPrice * 0.97, minBoughtPrice);
                var maxPrice = lastPrice * 1.04;

                var assetsToSell = allAssetsList
                    .Where(w => w.BoughtAtPrice.Price < orderPrice)
                    .ToList();

                if (assetsToSell.Any())
                {
                    int numberOfBuckets = 6;
                    int numberPerBucket = assetsToSell.Count() / numberOfBuckets;

                    var step = (maxPrice - orderPrice) / numberOfBuckets;

                    for (int i = 0; i < numberOfBuckets; i++)
                    {
                        var assetsInBatch = assetsToSell.Take(numberPerBucket);
                        yield return (orderPrice, assetsInBatch);
                        assetsToSell.RemoveAll(w => assetsInBatch.Contains(w));

                        orderPrice += step;
                    }

                    yield return (orderPrice, assetsToSell);
                }
            }
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellProfitablePeakAt99(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            return LeaveOldOrSellProfitablePeakAt99(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, allAssets, outsdandingBuyOrders, Enumerable.Empty<MarketplaceSellOrder>(), transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellProfitablePeakAtLatest(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            return LeaveOldOrSellProfitablePeakAtLatest(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, allAssets, outsdandingBuyOrders, Enumerable.Empty<MarketplaceSellOrder>(), transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellProfitablePeakAt101(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            return LeaveOldOrSellProfitablePeakAt101(currentTick, currentMoneyAvaliable, lastPrice, latest10Prices, allAssets, outsdandingBuyOrders, Enumerable.Empty<MarketplaceSellOrder>(), transactionCostCalculator);
        }

        //[SkipWhenFormingExecutionList(Skip = true)]
        public static IEnumerable<(double price, IEnumerable<Asset> assets)> CancelOldAndSellToGetLiquidity(int currentTick, double currentMoneyAvaliable, double lastPrice, IEnumerable<AssetPrice> latest10Prices, IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders, ITransactionCostCalculator transactionCostCalculator)
        {
            var allAssets = CancelOutstandingSellOrdersAndGetAllAssets(currentOwnedAssets, outsdandingSellOrders);

            if (!allAssets.Any())
            {
                yield return (-1, Enumerable.Empty<Asset>());
            }
            else
            {
                double price = -1;
                List<Asset> assetsToSell = new();

                //todo: ideally these thresholds would be in proportion to start money or asset price...
                if (currentMoneyAvaliable < 1000)
                {
                    assetsToSell = allAssets.OrderBy(w => w.BoughtAtPrice.Price).ToList();

                    (price, int numberToSell) = FindAssetsForSum(1000, 1);

                    assetsToSell = assetsToSell.Take(numberToSell).ToList();
                }
                else if (currentMoneyAvaliable < 2000)
                {
                    assetsToSell = allAssets.OrderBy(w => w.BoughtAtPrice.Price).ToList();

                    (price, int numberToSell) = FindAssetsForSum(2000, 1.02);

                    assetsToSell = assetsToSell.Take(numberToSell).ToList();
                }

                yield return (price, assetsToSell);

                (double price, int numberToSell) FindAssetsForSum(double demandedSum, double priceFactor)
                {
                    double sum = 0;
                    int numberToSell = 0;

                    for (numberToSell = 1; sum < demandedSum && numberToSell <= allAssets.Count; numberToSell++)
                    {
                        price = assetsToSell[numberToSell - 1].BoughtAtPrice.Price * priceFactor;
                        sum = numberToSell * price;
                    }

                    return (price, numberToSell);
                }
            }
        }
    }

    private static double CancelOutstandingBuyOrdersAndGetTotalMoney(double currentMoneyAvaliable, IEnumerable<MarketplaceBuyOrder> outsdandingBuyOrders)
    {
        var ordersToCancel = outsdandingBuyOrders.ToList();
        foreach (var buyOrder in ordersToCancel)
        {
            currentMoneyAvaliable += buyOrder.ReservedAssets;
            buyOrder.CancelOrder();
        }

        return currentMoneyAvaliable;
    }

    private static List<Asset> CancelOutstandingSellOrdersAndGetAllAssets(IEnumerable<Asset> currentOwnedAssets, IEnumerable<MarketplaceSellOrder> outsdandingSellOrders)
    {
        var allAssets = currentOwnedAssets.ToList();
        var toCancel = outsdandingSellOrders.ToList();

        foreach (var order in toCancel)
        {
            allAssets.AddRange(order.AssetsToSell);
            order.CancelOrder();
        }

        return allAssets;
    }

    class SkipWhenFormingExecutionListAttribute : Attribute
    {
        public bool Skip { get; set; } = true;
    }
}