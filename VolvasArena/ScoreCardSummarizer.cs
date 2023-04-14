using static BotArena;

static class ScoreCardSummarizer
{
    public static void PrintReport(List<TraderBotScoreCard>[] scorecards, Func<TraderBotScoreCard, double> kpiSelector, IOutputControl output)
    {
        int numberOfBots = scorecards.Length;
        int numberOfRoundsSimulated = scorecards[0].Count;

        var botNames = Enumerable.Range(0, numberOfBots).Select(w => $"{w}: {scorecards[w][0].Name}").ToArray();
        output.WriteLine(string.Join(Environment.NewLine, botNames));

        List<int> winners = new List<int>();

        for (int i = 0; i < numberOfRoundsSimulated; i++)
        {
            var scoresForRound = scorecards.Select(w => w[i]).ToList();

            var indexOfRoundWinner = scoresForRound.FindIndex(w => kpiSelector(w) == scoresForRound.Max(kpiSelector));

            winners.Add(indexOfRoundWinner);
        }

        var histogramBuckets = Enumerable.Range(-1, scorecards.Length + 1).Select(w => new HistogramBucket(w, w + 1));
        var histogram = new Histogram(histogramBuckets, winners.Select(w => (double)w));
        histogram.Print(output, maxStarsInColumn: 50);

        var winsPerBot = winners.GroupBy(w => w).Select(w => new { BotIndex = w.Key, NumberOfWins = w.Count() }).OrderByDescending(w => w.NumberOfWins).ToList();
        var overallWinnerIndex = winsPerBot.First().BotIndex;

        output.WriteLine("");
        output.WriteLine($"Winner: {botNames[overallWinnerIndex]}");
        output.WriteLine("Distribution");
        output.WriteLine("");

        var winnerScoreCards = scorecards[overallWinnerIndex];

        histogram = new Histogram(20, winnerScoreCards.Select(kpiSelector));
        histogram.Print(output, maxStarsInColumn: 50);

        Console.WriteLine();
        Console.WriteLine($"Average KPI over all runs:");

        foreach (var podiumMember in winsPerBot.OrderByDescending(w => w.NumberOfWins))
        {
            Console.WriteLine($"[{podiumMember.NumberOfWins} wins]: {botNames[podiumMember.BotIndex]}: Expected {scorecards[podiumMember.BotIndex].Select(kpiSelector).Average():N2} with {scorecards[podiumMember.BotIndex].Average(w => w.TotalNumberOfTransactions):N3} transactions on average");
        }
    }
}
