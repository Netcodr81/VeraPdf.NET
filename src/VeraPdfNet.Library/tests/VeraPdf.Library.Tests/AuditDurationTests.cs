using VeraPdf.Library.Components;

namespace VeraPdf.Library.Tests;

public class AuditDurationTests
{
    [Fact]
    public void CreatesDurationAndFormatsElapsedMilliseconds()
    {
        var duration = AuditDuration.FromValues(10_000, 10_000 + 3_600_000 + 23 * 60_000 + 45_678);

        Assert.Equal(10_000, duration.Start);
        Assert.Equal(5_025_678, duration.Difference);
        Assert.Equal("01:23:45.678", duration.Duration);
        Assert.Equal("01:23:45.678", AuditDuration.GetStringDuration(5_025_678));
    }

    [Fact]
    public void RejectsNegativeOrReversedEndpoints()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AuditDuration.FromValues(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => AuditDuration.FromValues(0, -1));
        Assert.Throws<ArgumentException>(() => AuditDuration.FromValues(2, 1));
    }

    [Fact]
    public void SumsNullAndOverlappingDurationCollections()
    {
        Assert.Equal(AuditDuration.FromValues(0, 0), AuditDuration.SumDuration(null));
        var result = AuditDuration.SumDuration(
        [
            AuditDuration.FromValues(14_000, 25_000),
            AuditDuration.FromValues(15_000, 30_000),
            AuditDuration.FromValues(12_000, 35_000),
            AuditDuration.FromValues(10_000, 27_000)
        ]);

        Assert.Equal(AuditDuration.FromValues(10_000, 35_000), result);
    }
}
