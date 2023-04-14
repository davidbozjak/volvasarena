interface ITransactionCostCalculator
{
    double TransactionCostToBuy(AssetPrice assetPrice, int amount);
    double TransactionCostToSell(IEnumerable<Asset> assetsToSell);
}

class AlwaysFreeTransactionCostCalculator : ITransactionCostCalculator
{
    public double TransactionCostToBuy(AssetPrice assetPrice, int amount)
    {
        return 0;
    }

    public double TransactionCostToSell(IEnumerable<Asset> assetsToSell)
    {
        return 0;
    }
}

class AlwaysOneTransactionCostCalculator : ITransactionCostCalculator
{
    public double TransactionCostToBuy(AssetPrice assetPrice, int amount)
    {
        return 1;
    }

    public double TransactionCostToSell(IEnumerable<Asset> assetsToSell)
    {
        return 1;
    }
}

// Avanza courtage descriptions (taken from 2023-01-21): https://www.avanza.se/konton-lan-prislista/prislista/courtageklasser.html

class AvanzaMiniCourtage : ITransactionCostCalculator
{
    public double TransactionCostToBuy(AssetPrice assetPrice, int amount)
    {
        return Math.Max(1, assetPrice.Price * amount * 0.0025);
    }

    public double TransactionCostToSell(IEnumerable<Asset> assetsToSell)
    {
        return Math.Max(1, assetsToSell.Sum(w => w.BoughtAtPrice.Price) * 0.0025);
    }
}

class AvanzaSmallCourtage : ITransactionCostCalculator
{
    public double TransactionCostToBuy(AssetPrice assetPrice, int amount)
    {
        return Math.Max(39, assetPrice.Price * amount * 0.0015);
    }

    public double TransactionCostToSell(IEnumerable<Asset> assetsToSell)
    {
        return Math.Max(39, assetsToSell.Sum(w => w.BoughtAtPrice.Price) * 0.0015);
    }
}

class AvanzaMediumCourtage : ITransactionCostCalculator
{
    public double TransactionCostToBuy(AssetPrice assetPrice, int amount)
    {
        return Math.Max(69, assetPrice.Price * amount * 0.00069);
    }

    public double TransactionCostToSell(IEnumerable<Asset> assetsToSell)
    {
        return Math.Max(69, assetsToSell.Sum(w => w.BoughtAtPrice.Price) * 0.00069);
    }
}

class AvanzaFastPrisCourtage : ITransactionCostCalculator
{
    public double TransactionCostToBuy(AssetPrice assetPrice, int amount)
    {
        return 99;
    }

    public double TransactionCostToSell(IEnumerable<Asset> assetsToSell)
    {
        return 99;
    }
}