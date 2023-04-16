using static BotArena;

interface ISimulationResultsReporter
{
    IReadOnlyList<TraderBotScoreCard>[] BotScoreCardsForAllRounds { get; }

    IReadOnlyList<double[]> PriceSeries { get; }

    void AddScorecard(TraderBotScoreCard[] scoreCards);

    void AddPriceDevelopment(IAssetPriceProvider assetPriceProvider);
}

class SimulationResultsReporter : ISimulationResultsReporter
{
    private readonly object lockingObject = new object();
    private readonly int reportInterval;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly DateTime startDateTime;
    private readonly IOutputControl outputControl;

    public int NumOfSimulationsToRun { get; }

    public int DoneCount { get; private set; }

    public bool ReportToFile { get; init; } = false;

    private readonly List<TraderBotScoreCard>[] scorecards;
    public IReadOnlyList<TraderBotScoreCard>[] BotScoreCardsForAllRounds => this.scorecards.Select(w => w.AsReadOnly()).ToArray();

    private readonly List<double[]> priceSeries;
    public IReadOnlyList<double[]> PriceSeries => this.priceSeries.AsReadOnly();

    private Task writeResultToFileTask = Task.CompletedTask;

    private string pricesLogFileName = string.Empty;
    private string scorecardsLogFileName = string.Empty;

    public SimulationResultsReporter(int numOfSimulationsToRun, ITraderBotFactory botFactory, IDateTimeProvider dateTimeProvider, IOutputControl outputControl, string runInfo)
    {
        this.NumOfSimulationsToRun = numOfSimulationsToRun;
        this.reportInterval = (int)Math.Sqrt(numOfSimulationsToRun);
        this.dateTimeProvider = dateTimeProvider;
        this.outputControl = outputControl;
        this.startDateTime = dateTimeProvider.Now;

        this.scorecards = Enumerable.Range(0, botFactory.NumberThatWillBeCreated).Select(w => new List<TraderBotScoreCard>()).ToArray();
        this.priceSeries = new List<double[]>();

        this.outputControl.WriteLine($"Preparing to run {numOfSimulationsToRun} of simulation, comparing {botFactory.NumberThatWillBeCreated} bots. {runInfo}");
    }

    public void AddScorecard(TraderBotScoreCard[] scoreCards)
    {
        lock (lockingObject)
        {
            for (int i = 0; i < scoreCards.Length; i++)
            {
                this.scorecards[i].Add(scoreCards[i]);
            }

            this.DoneCount++;

            if (this.DoneCount > this.NumOfSimulationsToRun)
                throw new Exception("Unexpected, done count should never exceed total number of simulations to run");
        }

        if (this.DoneCount < 5 || this.DoneCount % reportInterval == 0 ||this.DoneCount == this.NumOfSimulationsToRun)
        {
            var now = this.dateTimeProvider.Now;

            var ellapsed = now - startDateTime;
            var millisecondsPerSimulation = ellapsed.TotalMilliseconds / this.DoneCount;
            var remainingSimulationsToRun = this.NumOfSimulationsToRun - this.DoneCount;
            var approxRemainingMilliseconds = millisecondsPerSimulation * remainingSimulationsToRun;
            var ETA = now.AddMilliseconds(approxRemainingMilliseconds);

            this.outputControl.WriteLine($"{now:T}: Completed simulation {this.DoneCount} / {this.NumOfSimulationsToRun}. ETA: {ETA:T}, in ~{approxRemainingMilliseconds / 1000:N0} seconds");

            if (this.ReportToFile)
            {
                ChainWriteToFileAction(() => GenerateCurrentResultsSummaryToFile());
            }
        }

        if (this.ReportToFile)
        {
            ChainWriteToFileAction(() => AppendScoreCardsToFile(DoneCount, scoreCards));
        }
    }

    public void AddPriceDevelopment(IAssetPriceProvider assetPriceProvider)
    {
        var lastPrices = assetPriceProvider.AssetPrices.Select(w => w.Price).ToArray();

        lock (this.lockingObject)
        {
            this.priceSeries.Add(lastPrices);
        }

        if (this.ReportToFile)
        {
            ChainWriteToFileAction(() => AppendPriceToFile(lastPrices));
        }
    }

    private void ChainWriteToFileAction(Action writeToFileAction)
    {
        lock (this.lockingObject)
        {
            if (this.writeResultToFileTask.IsCompleted || this.writeResultToFileTask.IsCanceled || this.writeResultToFileTask.IsFaulted)
            {
                this.writeResultToFileTask = Task.Run(writeToFileAction);
            }
            else
            {
                this.writeResultToFileTask = this.writeResultToFileTask.ContinueWith(_ => writeToFileAction());
            }
        }
    }

    private void GenerateCurrentResultsSummaryToFile()
    {
        var partialResults = this.DoneCount != this.NumOfSimulationsToRun;
        using StreamWriter streamWriter = GetAnalyzedResultsStreamForWriting(partialResults);
        var fileOutput = new FileOutputControl(streamWriter);

        if (partialResults)
        {
            fileOutput.WriteLine($"Partial results, done {this.DoneCount} / {this.NumOfSimulationsToRun} simulations");
            fileOutput.WriteLine("");
        }
        
        //lock since we can't allow our results to change while we are generating results - we are passing ourselves to the summarizer!
        lock (this.lockingObject)
        {
            ScoreCardSummarizer.PrintReport(this, t => t.TotalRealizedProfit, fileOutput);
        }
    }

    private void AppendPriceToFile(double[] prices)
    {
        using StreamWriter streamWriter = GetPriceLogStreamForAppending();

        streamWriter.WriteLine(string.Join(';', prices));
    }

    private void AppendScoreCardsToFile(int simNum, TraderBotScoreCard[] scoreCards)
    {
        using StreamWriter streamWriter = GetScoreCardsStreamForAppending();

        streamWriter.WriteLine("");
        streamWriter.WriteLine($"Scorecards sim{simNum}");
        
        foreach (var scorecard in scoreCards)
        {
            streamWriter.WriteLine(scorecard);
        }
    }

    private StreamWriter GetPriceLogStreamForAppending()
    {
        var dir = EnsureResultsDirExists();

        if (string.IsNullOrWhiteSpace(this.pricesLogFileName))
        {
            this.pricesLogFileName = $"{dir.FullName}\\SimResults_{this.startDateTime.ToString("yyyy-MM-dd--HH-mm-ss")}_Prices.csv";
        }

        return new StreamWriter(this.pricesLogFileName, true);
    }

    private StreamWriter GetScoreCardsStreamForAppending()
    {
        var dir = EnsureResultsDirExists();

        if (string.IsNullOrWhiteSpace(this.pricesLogFileName))
        {
            this.scorecardsLogFileName = $"{dir.FullName}\\SimResults_{this.startDateTime.ToString("yyyy-MM-dd--HH-mm-ss")}_ScoreCards.csv";
        }

        return new StreamWriter(this.scorecardsLogFileName, true);
    }

    private StreamWriter GetAnalyzedResultsStreamForWriting(bool partialResult)
    {
        var dir = EnsureResultsDirExists();

        var filePath = $"{dir.FullName}\\SimResults_{this.startDateTime.ToString("yyyy-MM-dd--HH-mm-ss")}_AnalyzedResult{(partialResult ? "_Partial" : string.Empty)}.csv";

        return new StreamWriter(filePath, false);
    }

    private DirectoryInfo EnsureResultsDirExists()
    {
        var directoryName = "SimulationResults";

        if (!Directory.Exists(directoryName))
        {
            return Directory.CreateDirectory(directoryName);
        }
        else
        {
            return new DirectoryInfo(directoryName);
        }
    }
}