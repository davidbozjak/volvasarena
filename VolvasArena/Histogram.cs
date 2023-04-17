record HistogramBucket(double LowThreshold, double HighThreshold, string DisplayText)
{
    public HistogramBucket(double LowThreshold, double HighThreshold)
        :this(LowThreshold, HighThreshold, $"{LowThreshold.ToString("0.00")} - {HighThreshold.ToString("0.00")}")
    { }
}

class Histogram
{
    private readonly Dictionary<HistogramBucket, double[]> buckets;

    public int NumberOfBuckets { get; }

    public double[] this[int index]
    { 
        get
        {
            var keys = this.buckets.Keys.ToList();
            return this.buckets[keys[index]];
        }
    }

    public Histogram(IEnumerable<HistogramBucket> buckets, IEnumerable<double> values)
    {
        this.NumberOfBuckets = buckets.Count();
        this.buckets = InsertValuesIntoBuckets(buckets, values);
    }

    public Histogram(int numberOfBuckets, IEnumerable<double> values)
        : this(numberOfBuckets, values.ToArray())
    { }

    public Histogram(int numberOfBuckets, params double[] values)
    {
        this.NumberOfBuckets = numberOfBuckets;

        var orderedValues = values.OrderBy(w => w).ToArray();

        var minValue = orderedValues.First();
        var maxValue = orderedValues.Last();

        var step = 1e-5 + (maxValue - minValue) / numberOfBuckets;

        var bucketList = new List<HistogramBucket>();

        for (int i = 0; i < numberOfBuckets; i++)
        {
            bucketList.Add(new HistogramBucket(minValue, minValue + step));
            minValue += step;
        }

#if DEBUG
        if (minValue < maxValue)
            throw new Exception("Expecting we will cover at least the whole range");

        if (bucketList.Count != this.NumberOfBuckets)
            throw new Exception();

        for (int i = 1; i < bucketList.Count; i++)
        {
            var current = bucketList[i];

            if (current.LowThreshold >= current.HighThreshold)
                throw new Exception("On a single bucket low should always be smaller than high");

            var prev = bucketList[i - 1];

            if (current.LowThreshold != prev.HighThreshold)
                throw new Exception("Expecting buckets generating will be continous.");
        }
#endif

        this.buckets = InsertValuesIntoBuckets(bucketList, values);
    }

    private static Dictionary<HistogramBucket, double[]> InsertValuesIntoBuckets(IEnumerable<HistogramBucket> buckets, IEnumerable<double> values)
    {
        var dict = new Dictionary<HistogramBucket, double[]>();

        foreach (var bucket in buckets)
        {
            dict.Add(bucket,
                values.Where(w => w >= bucket.LowThreshold && w < bucket.HighThreshold).ToArray());
        }
        
#if DEBUG
        if (dict.Sum(w => w.Value.Length) != values.Count())
            throw new Exception("Expecting all items will be inserted");
#endif

        return dict;
    }

    public IEnumerable<(double minValue, double maxValue, string displayText, IEnumerable<double> values)> GetOrderedBuckets()
    {
        var keys = this.buckets.Keys.OrderBy(w => w.LowThreshold);

        foreach (var key in keys)
        {
            yield return (key.LowThreshold, key.HighThreshold, key.DisplayText, this.buckets[key]);
        }
    }

    public void PrintFigure(IOutputControl output, int? valueOfStar = null, int? maxStarsInColumn = null)
    {
#if DEBUG
        if (new[] { valueOfStar, maxStarsInColumn}.Select(w => w == null ? 1 : 0).Sum() != 1)
            throw new Exception("Expecting that at exactly one of these arguemtns will be provided");
#endif
        if (valueOfStar == null && maxStarsInColumn != null)
        {
            valueOfStar = (int)Math.Ceiling(this.buckets.Max(w => w.Value.Length) / (double)maxStarsInColumn);
        }

        int columnPadding = this.buckets.Keys.Select(w => w.DisplayText.Length).Max() + 1;

        foreach ((var low, var high, var displayText, var items) in this.GetOrderedBuckets())
        {
            output.Write($"{displayText.PadRight(columnPadding)}:  ");

            if (items.Any())
                output.Write("*");

            var numberOfStars = items.Count() / valueOfStar;

            for (int i = 0; i < numberOfStars; i++)
                output.Write("*");
            output.WriteLine(string.Empty);
        }

        if (valueOfStar != 1)
        {
            output.WriteLine($"Each * represents {valueOfStar} instances");
        }
    }

    public void PrintTable(IOutputControl output)
    {
        foreach ((var low, var high, var displayText, var items) in this.GetOrderedBuckets())
        {
            output.WriteLine($"{displayText}; {items.Count()}");
        }
    }
}