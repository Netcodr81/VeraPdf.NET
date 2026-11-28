using System.Globalization;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpDateTimeTests
{
    [Fact]
    public void CreatesEmptyAndUnzonedDates()
    {
        var empty = XmpDateTime.Create();
        Assert.False(empty.HasDate);
        Assert.False(empty.HasTime);
        Assert.False(empty.HasTimeZone);
        Assert.Equal("", empty.ToString());
        var date = XmpDateTime.Create(2024, 2, 29);
        Assert.True(date.HasDate);
        Assert.False(date.HasTime);
        Assert.False(date.HasTimeZone);
        Assert.Equal("2024-02-29", date.ToString());
        var time = XmpDateTime.Create(2024, 2, 29, 0, 0, 0, 123456789);
        Assert.True(time.HasTime);
        Assert.False(time.HasTimeZone);
        Assert.Equal("2024-02-29T00:00:00.123456789", time.ToString());
    }

    [Fact]
    public void DateFactoryClampsComponentRangesAndValidatesNanoseconds()
    {
        Assert.Equal("9999-01-31T23:59:59.999999999",
            XmpDateTime.Create(int.MinValue, 0, 99, int.MinValue, -100, int.MinValue, 999999999).ToString());
        Assert.Equal("0000-12-01T01:02:03", XmpDateTime.Create(0, 99, -1, -1, -2, -3, 0).ToString());
        Assert.Throws<ArgumentOutOfRangeException>(() => XmpDateTime.Create(2024, 1, 1, 0, 0, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XmpDateTime.Create(2024, 1, 1, 0, 0, 0, 1_000_000_000));
    }

    [Fact]
    public void AssignsAndRemovesOffsetsWithoutChangingComponents()
    {
        var source = XmpDateTime.Create(2024, 2, 29, 12, 30, 45, 123456789);
        var assigned = source.WithOffsetMinutes(330);
        Assert.False(source.HasTimeZone);
        Assert.Equal("2024-02-29T12:30:45.123456789+05:30", assigned.ToString());
        Assert.Equal("2024-02-29T07:00:45.123Z", assigned.ConvertToUtc().ToString());
        var changed = assigned.WithOffsetMinutes(-1439);
        Assert.Equal("2024-02-29T12:30:45.123456789-23:59", changed.ToString());
        Assert.Equal("2024-02-29T12:30:45.123456789", changed.WithOffsetMinutes(null).ToString());
        Assert.Equal("2024-02-29T12:30:45.123456789+05:30", assigned.ToString());
        Assert.Equal(0, XmpDateTime.Create().WithOffsetMinutes(0).OffsetMinutes);
        Assert.Throws<ArgumentOutOfRangeException>(() => source.WithOffsetMinutes(1440));
        Assert.Throws<ArgumentOutOfRangeException>(() => source.WithOffsetMinutes(-1440));
    }

    [Fact]
    public void ParseAndRenderMatchJavaReference()
    {
        foreach (var line in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-datetime-reference.tsv")))
        {
            var fields = line.Split('\t');
            if (fields[1] == "ERR")
            {
                var exception = Assert.Throws<XmpException>(() => XmpDateTime.Parse(fields[0]));
                Assert.Equal(int.Parse(fields[2], CultureInfo.InvariantCulture), (int)exception.Error);
                continue;
            }
            var date = XmpDateTime.Parse(fields[0]);
            var actual = new[]
            {
                date.ToString(), date.Year.ToString(CultureInfo.InvariantCulture),
                date.Month.ToString(CultureInfo.InvariantCulture), date.Day.ToString(CultureInfo.InvariantCulture),
                date.Hour.ToString(CultureInfo.InvariantCulture), date.Minute.ToString(CultureInfo.InvariantCulture),
                date.Second.ToString(CultureInfo.InvariantCulture), date.Nanosecond.ToString(CultureInfo.InvariantCulture),
                date.HasDate.ToString().ToLowerInvariant(), date.HasTime.ToString().ToLowerInvariant(),
                date.OffsetMinutes?.ToString(CultureInfo.InvariantCulture) ?? "none"
            };
            Assert.Equal(fields[2..], actual);
        }
    }

    [Fact]
    public void DistinguishesAbsentTimeAndZoneFromExplicitZeros()
    {
        var dateOnly = XmpDateTime.Parse("2024-01-02");
        var timeWithoutZone = XmpDateTime.Parse("2024-01-02T00:00");
        var utc = XmpDateTime.Parse("2024-01-02T00:00Z");
        Assert.False(dateOnly.HasTime);
        Assert.True(timeWithoutZone.HasTime);
        Assert.False(timeWithoutZone.HasTimeZone);
        Assert.True(utc.HasTimeZone);
        Assert.Equal(0, utc.OffsetMinutes);
    }

    [Fact]
    public void NullInputHasReferenceErrorCode()
    {
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => XmpDateTime.Parse(null!)).Error);
    }

    [Fact]
    public void UtcConversionsMatchJavaReference()
    {
        foreach (var line in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-utc-reference.tsv")))
        {
            var fields = line.Split('\t');
            var source = XmpDateTime.Parse(fields[0]);
            var original = source.ToString();
            var utc = source.ConvertToUtc();
            Assert.Equal(fields[1], utc.ToString());
            Assert.Equal(int.Parse(fields[2], CultureInfo.InvariantCulture), utc.Nanosecond);
            Assert.True(utc.HasDate && utc.HasTime && utc.HasTimeZone);
            Assert.Equal(0, utc.OffsetMinutes);
            Assert.Equal(original, source.ToString());
        }
    }

    [Fact]
    public void ConvertsFromDateTimeOffsetAtCalendarPrecision()
    {
        var source = new DateTimeOffset(2024, 2, 29, 23, 45, 59, TimeSpan.FromHours(5.5)).AddTicks(1234567);
        var date = XmpDateTime.FromDateTimeOffset(source);
        Assert.Equal("2024-02-29T23:45:59.123+05:30", date.ToString());
        Assert.Equal("2024-02-29T18:15:59.123Z", date.ConvertToUtc().ToString());
    }

    [Fact]
    public void ConvertsTimeZoneAtTheInstantAcrossDaylightTransitions()
    {
        var start = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 10);
        var end = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 11, 3);
        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), TimeSpan.FromHours(1), start, end);
        var zone = TimeZoneInfo.CreateCustomTimeZone("test-zone", TimeSpan.FromHours(-5), "test", "standard", "daylight", [rule]);
        var values = new[]
        {
            ("2024-03-10T06:59:59.999999999Z", "2024-03-10T01:59:59.999-05:00"),
            ("2024-03-10T07:00Z", "2024-03-10T03:00-04:00"),
            ("2024-11-03T05:30Z", "2024-11-03T01:30-04:00"),
            ("2024-11-03T06:30Z", "2024-11-03T01:30-05:00"),
            ("2024-03-11T03:00+23:00", "2024-03-09T23:00-05:00")
        };
        foreach (var (input, expected) in values)
        {
            var source = XmpDateTime.Parse(input);
            var original = source.ToString();
            var result = source.ConvertToTimeZone(zone);
            Assert.Equal(expected, result.ToString());
            Assert.Equal(original, source.ToString());
        }
    }

    [Fact]
    public void ConvertsToSystemLocalZone()
    {
        var source = XmpDateTime.Parse("2024-06-01T12:34:56.789123456Z");
        var expected = TimeZoneInfo.ConvertTime(new DateTimeOffset(2024, 6, 1, 12, 34, 56, 789, TimeSpan.Zero), TimeZoneInfo.Local);
        Assert.Equal(XmpDateTime.FromDateTimeOffset(expected).ToString(), source.ConvertToLocalTime().ToString());
        Assert.Throws<ArgumentNullException>(() => source.ConvertToTimeZone(null!));
    }

    [Fact]
    public void RejectsTimeZoneConversionOutsideCalendarRange()
    {
        var negative = TimeZoneInfo.CreateCustomTimeZone("negative", TimeSpan.FromHours(-1), "negative", "negative");
        var positive = TimeZoneInfo.CreateCustomTimeZone("positive", TimeSpan.FromHours(1), "positive", "positive");
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() =>
            XmpDateTime.Parse("0001-01-01T00:00Z").ConvertToTimeZone(negative)).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() =>
            XmpDateTime.Parse("9999-12-31T23:59Z").ConvertToTimeZone(positive)).Error);
    }

    [Theory]
    [InlineData("2024-01-02T03:04:05.123456789Z", "2024-01-02T08:34:05.123456789+05:30", 0)]
    [InlineData("2024-01-02T03:04:05.123456789Z", "2024-01-02T03:04:05.123456788Z", 1)]
    [InlineData("2024-01-02T03:04:05.123000001Z", "2024-01-02T03:04:05.123999999Z", -1)]
    [InlineData("2024-01-02T03:04:05.999999999Z", "2024-01-02T03:04:06Z", -1)]
    [InlineData("2024-01-01T00:00+23:59", "2023-12-31T00:01Z", 0)]
    [InlineData("0001-01-01T00:00Z", "9999-12-31T23:59Z", -1)]
    public void ComparesInstantsAndNanoseconds(string left, string right, int expected)
    {
        var a = XmpDateTime.Parse(left);
        var b = XmpDateTime.Parse(right);
        Assert.Equal(expected, Math.Sign(a.CompareTo(b)));
        Assert.Equal(-expected, Math.Sign(b.CompareTo(a)));
        Assert.Equal(left, a.ToString());
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("2024")]
    [InlineData("2024-02-29")]
    [InlineData("2024-02-29T01:02")]
    [InlineData("2024-02-31T01:02Z")]
    [InlineData("0000-01-01T01:02Z")]
    [InlineData("0001-01-01T00:00+23:59")]
    [InlineData("9999-12-31T23:59-23:59")]
    public void UtcConversionRejectsUnsupportedDates(string value)
    {
        var date = XmpDateTime.Parse(value);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => date.ConvertToUtc()).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => date.ConvertToTimeZone(TimeZoneInfo.Utc)).Error);
        var valid = XmpDateTime.Parse("2024-01-01T00:00Z");
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => date.CompareTo(valid)).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => valid.CompareTo(date)).Error);
    }
}
