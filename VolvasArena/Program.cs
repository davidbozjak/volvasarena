using static BotArena;

var randomProvider = new RandomProvider();
var dateTimeProvider = new DateTimeProvider();
var output = new ConsoleOutputControl();
BuyAndSellStrategy.RandomProvider = randomProvider;

AssetType assetType = new("A");
double startMoney = 10000;
double startAssetPrice = 100;
int numOfSimulationsToRun = 100;
int simulateTicks = 200;

var buyStrategies = BuyAndSellStrategy.GetBuySrategies().ToArray();
var sellStrategies = BuyAndSellStrategy.GetSellSrategies().ToArray();

ITraderBotFactory factory = new BotArena.DifferentStrategiesFactory(startMoney, assetType, buyStrategies, sellStrategies);
var reporter = new SimulationResultsReporter(numOfSimulationsToRun, factory, dateTimeProvider, output, "BALANCED") { ReportSummariesToFile = true };

BotArena.CompareStrategies(assetType, startAssetPrice, simulateTicks, numOfSimulationsToRun, new AlwaysFreeTransactionCostCalculator(),
    GetBalancedPriceSimulator,
    factory,
    reporter);
ScoreCardSummarizer.PrintReport(reporter, scoreCard => scoreCard.TotalRealizedProfit, output);

factory = new BotArena.DifferentStrategiesFactory(startMoney, assetType, buyStrategies, sellStrategies);
reporter = new SimulationResultsReporter(numOfSimulationsToRun, factory, dateTimeProvider, output, "FALLING");

BotArena.CompareStrategies(assetType, startAssetPrice, simulateTicks, numOfSimulationsToRun, new AvanzaMiniCourtage(),
    GetSlowlyFallingPriceSimulator,
    factory,
    reporter);
ScoreCardSummarizer.PrintReport(reporter, scoreCard => scoreCard.TotalRealizedProfit, output);

factory = new BotArena.DifferentStrategiesFactory(startMoney, assetType, buyStrategies, sellStrategies);
reporter = new SimulationResultsReporter(numOfSimulationsToRun, factory, dateTimeProvider, output, "RAISING");

BotArena.CompareStrategies(assetType, startAssetPrice, simulateTicks, numOfSimulationsToRun, new AvanzaMiniCourtage(),
    GetSlowlyRisingPriceSimulator,
    factory,
    reporter);
ScoreCardSummarizer.PrintReport(reporter, scoreCard => scoreCard.TotalRealizedProfit, output);

factory = new BotArena.DifferentStartMoneyFactory(1000, 10000, 5, assetType, BuyAndSellStrategy.BuyStrategies.BuyRandomAmountAtLastPrice, BuyAndSellStrategy.SellStrategies.SellRandomAmountOfProfitableAtLastPrice);
reporter = new SimulationResultsReporter(numOfSimulationsToRun, factory, dateTimeProvider, output, "Different starting assets");

BotArena.CompareStrategies(assetType, startAssetPrice, simulateTicks, numOfSimulationsToRun, new AvanzaMiniCourtage(),
    GetSlowlyFallingPriceSimulator,
    factory,
    reporter);
ScoreCardSummarizer.PrintReport(reporter, scoreCard => scoreCard.TotalRealizedProfit, output);

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

Console.WriteLine("Pick strategy to run on historical data:");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Pick BUY strategy:");

int selectionIndex = 0;
Console.WriteLine(string.Join(Environment.NewLine, buyStrategies.Select(w => $"{selectionIndex++}> {w.Item1}")));
int buyStrategyId = int.Parse(Console.ReadLine());

Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Pick SELL strategy:");

selectionIndex = 0;
Console.WriteLine(string.Join(Environment.NewLine, sellStrategies.Select(w => $"{selectionIndex++}> {w.Item1}")));
int sellStrategyId = int.Parse(Console.ReadLine());

var botFactory = new BotArena.DifferentStrategiesFactory(startMoney, assetType, new[] { buyStrategies[buyStrategyId] }, new[] { sellStrategies[sellStrategyId] });

Console.WriteLine();
Console.WriteLine();
Console.WriteLine("File path to simualte on: ");
string filePath = Console.ReadLine();

var historicalDataProvider = new AssetPriceSimpleCSVReader(new AssetType(""), filePath);

(var scorecards, _) = BotArena.RunOneSimulation(double.NaN, historicalDataProvider.AssetType, historicalDataProvider.TotalTicksAvaliable - Marketplace.TicksOfHistoryToProvide,
    new AvanzaMiniCourtage(), (_, _) => historicalDataProvider, botFactory, new BotArena.TraderBotEvaluator());

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine($"Bot Stats after {historicalDataProvider.TotalTicksAvaliable} ticks:");
Console.WriteLine(scorecards[0]);

double SimulatePriceMove(IAssetPriceProvider assetPriceProvider)
{
    for (int i = 0; i < simulateTicks; i++)
    {
        assetPriceProvider.MakeTick();

        var price = assetPriceProvider.LatestAssetPrice;
    }

#if DEBUG
    if (assetPriceProvider.TicksSimulated != simulateTicks)
        throw new Exception();
#endif

    return assetPriceProvider.LatestAssetPriceValue;
}

AssetPriceSimulator GetBalancedPriceSimulator(double startPrice, AssetType assetType)
{
    return new AssetPriceSimulator(assetType, startPrice, randomProvider, new[]
    {
        new PriceChangeAlternative(0.3, w => w),
        new PriceChangeAlternative(0.3, w => w * 1.01),
        new PriceChangeAlternative(0.3, w => w * 0.99),
        new PriceChangeAlternative(0.04, w => w * 1.03),
        new PriceChangeAlternative(0.04, w => w * 0.97),
        new PriceChangeAlternative(0.01, w => w * 1.05),
        new PriceChangeAlternative(0.01, w => w * 0.95)
    });
}

AssetPriceSimulator GetSlowlyRisingPriceSimulator(double startPrice, AssetType assetType)
{
    return new AssetPriceSimulator(assetType, startPrice, randomProvider, new[]
    {
        new PriceChangeAlternative(0.3, w => w),
        new PriceChangeAlternative(0.4, w => w * 1.01),
        new PriceChangeAlternative(0.2, w => w * 0.99),
        new PriceChangeAlternative(0.06, w => w * 1.03),
        new PriceChangeAlternative(0.02, w => w * 0.97),
        new PriceChangeAlternative(0.015, w => w * 1.05),
        new PriceChangeAlternative(0.005, w => w * 0.95)
    });
}

AssetPriceSimulator GetSlowlyFallingPriceSimulator(double startPrice, AssetType assetType)
{
    return new AssetPriceSimulator(assetType, startPrice, randomProvider, new[]
    {
        new PriceChangeAlternative(0.3, w => w),
        new PriceChangeAlternative(0.2, w => w * 1.01),
        new PriceChangeAlternative(0.4, w => w * 0.99),
        new PriceChangeAlternative(0.02, w => w * 1.03),
        new PriceChangeAlternative(0.06, w => w * 0.97),
        new PriceChangeAlternative(0.005, w => w * 1.05),
        new PriceChangeAlternative(0.015, w => w * 0.95)
    });
}

AssetPriceSimpleCSVReader GetHistoricalPriceProvider()
{
    return new AssetPriceSimpleCSVReader(new AssetType(""), @"C:\Users\bozja\OneDrive\Desktop\HistoricalStockPrices.csv");
}