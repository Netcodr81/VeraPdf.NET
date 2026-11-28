using System.Globalization;

namespace VeraPdf.Library.Components;

public sealed class AuditDuration : IEquatable<AuditDuration>
{
    public static AuditDuration Default { get; } = new(0, 0);

    private AuditDuration(long start, long finish)
    {
        Start = start;
        Finish = finish;
    }

    public long Start { get; }
    public long Finish { get; }
    public long Difference => Finish - Start;
    public string Duration => GetStringDuration(Difference);

    public static AuditDuration FromValues(long start, long finish)
    {
        if (start < 0 || finish < 0)
            throw new ArgumentOutOfRangeException(nameof(start), "Start and finish must be non-negative.");
        if (start > finish)
            throw new ArgumentException("Start must not be greater than finish.", nameof(start));
        return new AuditDuration(start, finish);
    }

    public static string GetStringDuration(long difference)
    {
        var hours = difference / 3_600_000;
        var minutes = difference / 60_000 % 60;
        var seconds = difference / 1_000 % 60;
        var milliseconds = difference % 1_000;
        return string.Create(CultureInfo.InvariantCulture,
            $"{hours:D2}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}");
    }

    public static AuditDuration SumDuration(IEnumerable<AuditDuration>? durations)
    {
        long start = 0;
        long finish = 0;
        if (durations is not null)
        {
            foreach (var duration in durations)
            {
                ArgumentNullException.ThrowIfNull(duration);
                if (start == 0 || duration.Start < start)
                    start = duration.Start;
                if (duration.Finish > finish)
                    finish = duration.Finish;
            }
        }
        return FromValues(start, finish);
    }

    public bool Equals(AuditDuration? other) =>
        other is not null && Start == other.Start && Finish == other.Finish;

    public override bool Equals(object? obj) => obj is AuditDuration other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Start, Finish);

    public override string ToString() =>
        $"AuditDuration [start={Start}, finish={Finish}, getDifference()={Difference}, getDuration()={Duration}]";
}
