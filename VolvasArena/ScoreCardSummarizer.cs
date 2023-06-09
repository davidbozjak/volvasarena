﻿using static BotArena;

static class ScoreCardSummarizer
{
    public static void PrintReportFigure(ISimulationResultsReporter simulationResultsReporter, Func<TraderBotScoreCard, double> kpiSelector, IOutputControl output)
    {
        var scorecards = simulationResultsReporter.BotScoreCardsForAllRounds;

        int numberOfBots = scorecards.Length;
        int numberOfRoundsSimulated = scorecards[0].Count;

        var botNames = Enumerable.Range(0, numberOfBots).Select(w => $"{w}: {scorecards[w][0].Name}").ToArray();

        List<int> winners = new List<int>();

        for (int i = 0; i < numberOfRoundsSimulated; i++)
        {
            var scoresForRound = scorecards.Select(w => w[i]).ToList();

            var indexOfRoundWinner = scoresForRound.FindIndex(w => kpiSelector(w) == scoresForRound.Max(kpiSelector));

            winners.Add(indexOfRoundWinner);
        }

        var histogramBuckets = Enumerable.Range(0, scorecards.Length).Select(w => new HistogramBucket(w, w + 1, botNames[w]));
        var histogram = new Histogram(histogramBuckets, winners.Select(w => (double)w));
        histogram.PrintFigure(output, maxStarsInColumn: 50);

        var winsPerBot = winners.GroupBy(w => w).Select(w => new { BotIndex = w.Key, NumberOfWins = w.Count() }).OrderByDescending(w => w.NumberOfWins).ToList();
        var overallWinnerIndex = winsPerBot.First().BotIndex;

        output.WriteLine("");
        output.WriteLine($"Winner: {botNames[overallWinnerIndex]}");
        output.WriteLine("Distribution");
        output.WriteLine("");

        var winnerScoreCards = scorecards[overallWinnerIndex];

        histogram = new Histogram(20, winnerScoreCards.Select(kpiSelector));
        histogram.PrintFigure(output, maxStarsInColumn: 50);

        output.WriteLine("");
        output.WriteLine($"Average KPI over all runs:");

        foreach (var podiumMember in winsPerBot.OrderByDescending(w => w.NumberOfWins))
        {
            output.WriteLine($"[{podiumMember.NumberOfWins} wins]: {botNames[podiumMember.BotIndex]}: Expected {scorecards[podiumMember.BotIndex].Select(kpiSelector).Average():N2} with {scorecards[podiumMember.BotIndex].Average(w => w.TotalNumberOfTransactions):N3} transactions on average");
        }

        output.WriteLine("");
        output.WriteLine("Asset price development during this time:");

        var finalPrices = simulationResultsReporter.PriceSeries.Select(w => w.Last()).ToList();
        var priceHistogram = new Histogram(20, finalPrices);
        priceHistogram.PrintFigure(output, maxStarsInColumn: 30);
    }

    public static void PrintReportTable(ISimulationResultsReporter simulationResultsReporter, Func<TraderBotScoreCard, double> kpiSelector, IOutputControl output)
    {
        var scorecards = simulationResultsReporter.BotScoreCardsForAllRounds;

        int numberOfBots = scorecards.Length;
        int numberOfRoundsSimulated = scorecards[0].Count;

        var botNames = Enumerable.Range(0, numberOfBots).Select(w => $"{w}: {scorecards[w][0].Name}").ToArray();

        List<int> winners = new List<int>();

        for (int i = 0; i < numberOfRoundsSimulated; i++)
        {
            var scoresForRound = scorecards.Select(w => w[i]).ToList();

            var indexOfRoundWinner = scoresForRound.FindIndex(w => kpiSelector(w) == scoresForRound.Max(kpiSelector));

            winners.Add(indexOfRoundWinner);
        }

        var histogramBuckets = Enumerable.Range(0, scorecards.Length).Select(w => new HistogramBucket(w, w + 1, botNames[w]));
        var histogram = new Histogram(histogramBuckets, winners.Select(w => (double)w));
        histogram.PrintTable(output);

        var winsPerBot = winners.GroupBy(w => w).Select(w => new { BotIndex = w.Key, NumberOfWins = w.Count() }).OrderByDescending(w => w.NumberOfWins).ToList();
        var overallWinnerIndex = winsPerBot.First().BotIndex;

        output.WriteLine("");
        output.WriteLine($"Winner: {botNames[overallWinnerIndex]}");
        output.WriteLine("Distribution");
        output.WriteLine("");

        var winnerScoreCards = scorecards[overallWinnerIndex];

        histogram = new Histogram(20, winnerScoreCards.Select(kpiSelector));
        histogram.PrintTable(output);

        output.WriteLine("");
        output.WriteLine($"Average KPI over all runs:");

        foreach (var podiumMember in winsPerBot.OrderByDescending(w => w.NumberOfWins))
        {
            output.WriteLine($"[{podiumMember.NumberOfWins} wins]: {botNames[podiumMember.BotIndex]}: Expected {scorecards[podiumMember.BotIndex].Select(kpiSelector).Average():N2} with {scorecards[podiumMember.BotIndex].Average(w => w.TotalNumberOfTransactions):N3} transactions on average");
        }

        output.WriteLine("");
        output.WriteLine("Asset price development during this time:");

        var finalPrices = simulationResultsReporter.PriceSeries.Select(w => w.Last()).ToList();
        var priceHistogram = new Histogram(20, finalPrices);
        priceHistogram.PrintTable(output);
    }
}
