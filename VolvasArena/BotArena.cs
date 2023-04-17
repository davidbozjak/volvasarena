using System.Text;

class BotArena
{
    public static void CompareStrategies(AssetType assetType, double startAssetPrice, int ticksInRound, int numOfSimulationsToRun, ITransactionCostCalculator transactionCostCalculator,
        Func<double, AssetType, IAssetPriceProvider> assetPriceProviderFactory,
        ITraderBotFactory botFactory,
        ISimulationResultsReporter simulationResultsReporter)
    {
        var botEvaluator = new TraderBotEvaluator();

        Parallel.For(0, numOfSimulationsToRun, i =>
        {
            simulationResultsReporter.ReportStartingSimulation(i);

            (var scoreCards, var assetPriceProvider) = RunOneSimulation(startAssetPrice, assetType, ticksInRound, transactionCostCalculator, assetPriceProviderFactory, botFactory, botEvaluator);

            simulationResultsReporter.ReportFinishedSimulation(i);

            simulationResultsReporter.AddScorecard(scoreCards);
            simulationResultsReporter.AddPriceDevelopment(assetPriceProvider);

#if !DEBUG
            GC.Collect();
#endif
        });
    }

    public static (TraderBotScoreCard[], IAssetPriceProvider) RunOneSimulation(double startAssetPrice, AssetType assetType, int ticksInRound, ITransactionCostCalculator transactionCostCalculator, Func<double, AssetType, IAssetPriceProvider> assetPriceProviderFactory, ITraderBotFactory botFactory, ITraderBotEvaluator evaluator)
    {
        var assetPriceProvider = assetPriceProviderFactory(startAssetPrice, assetType);

        var marketplace = new Marketplace(assetPriceProvider, transactionCostCalculator);
        marketplace.RunForInitialPeriod();

        var bots = botFactory.Create().ToArray();

        marketplace.SubscribeRange(bots.Select(w => w.Bot));

        for (int tick = 0; tick < ticksInRound; tick++)
        {
            marketplace.MakeTick();
        }

        var scorecards = bots.Select(evaluator.Evaluate).ToArray();

        foreach (var botBirthCertificate in bots)
        {
            botBirthCertificate.Bot.Dispose();
        }

        return (scorecards, assetPriceProvider);
    }

    public interface ITraderBotFactory
    {
        int NumberThatWillBeCreated { get; }

        IEnumerable<TraderBotBirthCeritificate> Create();
    }

    public class DifferentStrategiesFactory : ITraderBotFactory
    {
        private readonly double startMoney;
        private readonly AssetType assetType;
        private readonly (string, TraderBot.GetAmountToBuyDelegate)[] buyStrategies;
        private readonly (string, TraderBot.GetAssetsToSellDelegate)[] sellStrategies;

        public DifferentStrategiesFactory(double startMoney, AssetType assetType,
            IEnumerable<(string, TraderBot.GetAmountToBuyDelegate)> buyStrategies,
            IEnumerable<(string, TraderBot.GetAssetsToSellDelegate)> sellStrategies)
        {
            this.startMoney = startMoney;
            this.assetType = assetType;
            this.buyStrategies = buyStrategies.ToArray();
            this.sellStrategies = sellStrategies.ToArray();
        }

        public int NumberThatWillBeCreated => this.buyStrategies.Length * this.sellStrategies.Length;

        public IEnumerable<TraderBotBirthCeritificate> Create()
        {
            foreach ((var nameB, var funcB) in buyStrategies)
            {
                foreach ((var nameS, var funcS) in sellStrategies)
                {
                    string name = $"{nameB} - {nameS}";

                    yield return new TraderBotBirthCeritificate(new TraderBot(name, startMoney, assetType, funcB, funcS), startMoney);
                }
            }
        }
    }

    public class DifferentStartMoneyFactory : ITraderBotFactory
    {
        private readonly double minValue;
        private readonly double maxValue;
        private readonly int numberToCreate;
        private readonly AssetType assetType;
        private readonly TraderBot.GetAmountToBuyDelegate buyStrategy;
        private readonly TraderBot.GetAssetsToSellDelegate sellStrategy;

        public DifferentStartMoneyFactory(double minValue, double maxValue, int numberToCreate, AssetType assetType, TraderBot.GetAmountToBuyDelegate buyStrategy, TraderBot.GetAssetsToSellDelegate sellStrategy)
        {
            if (maxValue <= minValue)
                throw new Exception();

            this.minValue = minValue;
            this.maxValue = maxValue;
            this.numberToCreate = numberToCreate;
            this.assetType = assetType;
            this.buyStrategy = buyStrategy;
            this.sellStrategy = sellStrategy;
        }

        public int NumberThatWillBeCreated => this.numberToCreate;

        public IEnumerable<TraderBotBirthCeritificate> Create()
        {
            var diff = this.maxValue - this.minValue;
            var step = diff / (numberToCreate - 1);
            var current = minValue;

            for (int i = 0; i < numberToCreate; i++)
            {
                yield return new TraderBotBirthCeritificate(new TraderBot($"M{current:N1}", current, assetType, buyStrategy, sellStrategy), current);
                current += step;
            }
        }
    }

    public record TraderBotBirthCeritificate(TraderBot Bot, double StartMoney);

    public interface ITraderBotEvaluator
    {
        TraderBotScoreCard Evaluate(TraderBotBirthCeritificate ceritificate);
    }

    public class TraderBotEvaluator : ITraderBotEvaluator
    {
        public TraderBotScoreCard Evaluate(TraderBotBirthCeritificate ceritificate)
        {
            return new TraderBotScoreCard(ceritificate);
        }
    }

    public class TraderBotScoreCard
    {
        public string Name { get; }

        public double TotalRealizedProfit { get; }
        public double RelativeRealizedProfit { get; }
        public double CurrentTotalAssets { get; }
        public double TotalTransactionCost { get; }
        public int TotalNumberOfTransactions { get; }
        public double InitialAvaliableFunds { get; }

        public TraderBotScoreCard(TraderBotBirthCeritificate ceritificate)
        {
            this.Name = ceritificate.Bot.Name;

            this.InitialAvaliableFunds = ceritificate.StartMoney;
            this.TotalRealizedProfit = ceritificate.Bot.TotalRealizedProfit;
            this.RelativeRealizedProfit = this.TotalRealizedProfit / this.InitialAvaliableFunds;
            this.CurrentTotalAssets = ceritificate.Bot.CurrentTotalAssets;
            this.TotalNumberOfTransactions = ceritificate.Bot.CompletedTransactions.Count();
            this.TotalTransactionCost = ceritificate.Bot.TotalTransactionCost;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Name: {this.Name} with {this.InitialAvaliableFunds} avaliable at the start");
            builder.AppendLine($"Total realized profit: {this.TotalRealizedProfit:N2}");
            builder.AppendLine($"Relative realized profit: {(this.RelativeRealizedProfit*100):N2}");
            builder.AppendLine($"CurrentTotalAssets (if liquified at current price): {this.CurrentTotalAssets}");
            builder.AppendLine($"Transaction costs: {this.TotalTransactionCost}");

            return builder.ToString();
        }
    }
}
