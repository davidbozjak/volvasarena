using static BotArena;

interface IScorecardReporter
{
    void AddScorecard(TraderBotScoreCard[] scoreCards);
}

class ScorecardReporter : IScorecardReporter
{
    private readonly object lockingObject = new object();
    private readonly int reportInterval;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly DateTime startDateTime;
    private readonly IOutputControl outputControl;

    public int NumOfSimulationsToRun { get; }

    public int DoneCount { get; private set; }

    public List<TraderBotScoreCard>[] BotScoreCardsForAllRounds { get; }

    public ScorecardReporter(int numOfSimulationsToRun, ITraderBotFactory botFactory, IDateTimeProvider dateTimeProvider, IOutputControl outputControl, string runInfo)
    {
        this.NumOfSimulationsToRun = numOfSimulationsToRun;
        this.BotScoreCardsForAllRounds = Enumerable.Range(0, botFactory.NumberThatWillBeCreated).Select(w => new List<TraderBotScoreCard>()).ToArray();
        this.reportInterval = (int)Math.Sqrt(numOfSimulationsToRun);
        this.dateTimeProvider = dateTimeProvider;
        this.outputControl = outputControl;

        this.startDateTime = dateTimeProvider.Now;

        this.outputControl.WriteLine($"Preparing to run {numOfSimulationsToRun} of simulation, comparing {botFactory.NumberThatWillBeCreated} bots. {runInfo}");
    }

    public void AddScorecard(TraderBotScoreCard[] scoreCards)
    {
        lock (lockingObject)
        {
            for (int i = 0; i < scoreCards.Length; i++)
            {
                this.BotScoreCardsForAllRounds[i].Add(scoreCards[i]);
            }

            this.DoneCount++;

            if (this.DoneCount > this.NumOfSimulationsToRun)
                throw new Exception("Unexpected, done count should never exceed total number of simulations to run");

            if (this.DoneCount < 5 || this.DoneCount % reportInterval == 0)
            {
                var now = this.dateTimeProvider.Now;

                var ellapsed = now - startDateTime;
                var millisecondsPerSimulation = ellapsed.TotalMilliseconds / this.DoneCount;
                var remainingSimulationsToRun = this.NumOfSimulationsToRun - this.DoneCount;
                var approxRemainingMilliseconds = millisecondsPerSimulation * remainingSimulationsToRun;
                var ETA = now.AddMilliseconds(approxRemainingMilliseconds);

                this.outputControl.WriteLine($"{now:T}: Completed simulation {this.DoneCount} / {this.NumOfSimulationsToRun}. ETA: {ETA:T}, in ~{approxRemainingMilliseconds / 1000:N0} seconds");
            }
        }
    }
}