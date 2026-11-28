// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Globalization;
using System.Text;

namespace VeraPdf.Library.Xmp;

/// <summary>
/// An XMP date parsed without assuming a time zone or requiring a complete calendar date.
/// Components are normalized according to the reference XMP toolkit, not calendar validation.
/// </summary>
public sealed class XmpDateTime : IComparable<XmpDateTime>
{
    private XmpDateTime() { }

    public int Year { get; private set; }
    public int Month { get; private set; }
    public int Day { get; private set; }
    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public int Second { get; private set; }
    public int Nanosecond { get; private set; }
    public bool HasDate { get; private set; }
    public bool HasTime { get; private set; }
    public bool HasTimeZone => OffsetMinutes.HasValue;
    public int? OffsetMinutes { get; private set; }

    /// <summary>Creates an empty date with no date, time, or time-zone components.</summary>
    public static XmpDateTime Create() => new();

    /// <summary>Creates a date without a time or time zone, clamping components to the XMP ranges.</summary>
    public static XmpDateTime Create(int year, int month, int day) => new()
    {
        Year = ClampMagnitude(year, 9999),
        Month = Math.Clamp(month, 1, 12),
        Day = Math.Clamp(day, 1, 31),
        HasDate = true
    };

    /// <summary>Creates an unzoned date and time, clamping date and time components to the XMP ranges.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Nanoseconds are outside the range 0 through 999,999,999.</exception>
    public static XmpDateTime Create(int year, int month, int day, int hour, int minute, int second, int nanosecond)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nanosecond);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nanosecond, 999_999_999);
        var date = Create(year, month, day);
        date.Hour = ClampMagnitude(hour, 23);
        date.Minute = ClampMagnitude(minute, 59);
        date.Second = ClampMagnitude(second, 59);
        date.Nanosecond = nanosecond;
        date.HasTime = true;
        return date;
    }

    /// <summary>Returns a copy with a fixed offset, without adjusting its date or time components.</summary>
    /// <param name="offsetMinutes">Offset in minutes from -1439 to 1439, or null to remove the offset.</param>
    /// <exception cref="ArgumentOutOfRangeException">The offset is outside the supported range.</exception>
    public XmpDateTime WithOffsetMinutes(int? offsetMinutes)
    {
        if (offsetMinutes is < -1439 or > 1439)
            throw new ArgumentOutOfRangeException(nameof(offsetMinutes));
        return new XmpDateTime
        {
            Year = Year, Month = Month, Day = Day, Hour = Hour, Minute = Minute, Second = Second,
            Nanosecond = Nanosecond, HasDate = HasDate, HasTime = HasTime, OffsetMinutes = offsetMinutes
        };
    }

    private static int ClampMagnitude(int value, int maximum) => (int)Math.Min(Math.Abs((long)value), maximum);

    /// <summary>
    /// Creates a complete date from a fixed-offset instant, retaining milliseconds as in the
    /// reference calendar conversion. Finer precision is discarded.
    /// </summary>
    public static XmpDateTime FromDateTimeOffset(DateTimeOffset value) => new()
    {
        Year = value.Year,
        Month = value.Month,
        Day = value.Day,
        Hour = value.Hour,
        Minute = value.Minute,
        Second = value.Second,
        Nanosecond = value.Millisecond * 1_000_000,
        HasDate = true,
        HasTime = true,
        OffsetMinutes = (int)value.Offset.TotalMinutes
    };

    /// <summary>
    /// Converts a complete, valid, explicitly zoned date to UTC. The reference calendar
    /// conversion retains milliseconds and discards finer precision.
    /// </summary>
    /// <exception cref="XmpException">The date is incomplete, invalid, unzoned, or outside the supported calendar range.</exception>
    public XmpDateTime ConvertToUtc() => FromDateTimeOffset(new DateTimeOffset(GetUtcDateTime()));

    /// <summary>
    /// Converts the represented instant to a time zone, using its offset at that instant.
    /// The original instance is unchanged and sub-millisecond precision is discarded.
    /// </summary>
    /// <exception cref="XmpException">The date is incomplete, invalid, unzoned, or outside the supported calendar range.</exception>
    public XmpDateTime ConvertToTimeZone(TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        var utc = new DateTimeOffset(GetUtcDateTime());
        var offset = timeZone.GetUtcOffset(utc);
        try
        {
            return FromDateTimeOffset(utc.ToOffset(offset));
        }
        catch (ArgumentException exception)
        {
            throw new XmpException("Time zone conversion is outside the supported calendar range", XmpError.BadValue, exception);
        }
    }

    /// <summary>Converts the represented instant to the system's local time zone.</summary>
    public XmpDateTime ConvertToLocalTime() => ConvertToTimeZone(TimeZoneInfo.Local);

    /// <summary>
    /// Compares complete, valid, explicitly zoned dates by instant, including nanosecond precision.
    /// A non-null date sorts after null.
    /// </summary>
    /// <exception cref="XmpException">Either date is incomplete, invalid, unzoned, or outside the supported calendar range.</exception>
    public int CompareTo(XmpDateTime? other)
    {
        if (other is null) return 1;
        var comparison = GetUtcDateTime().CompareTo(other.GetUtcDateTime());
        return comparison != 0 ? comparison : Nanosecond.CompareTo(other.Nanosecond);
    }

    private DateTime GetUtcDateTime()
    {
        if (!HasDate || !HasTime || OffsetMinutes is not int offset || Year < 1 || Month < 1 || Day < 1 ||
            Day > DateTime.DaysInMonth(Year, Month))
            throw new XmpException("Time zone conversion requires a complete, valid date with a time zone", XmpError.BadValue);
        try
        {
            var local = new DateTime(Year, Month, Day, Hour, Minute, Second, Nanosecond / 1_000_000,
                DateTimeKind.Unspecified);
            var utc = local.AddMinutes(-offset);
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new XmpException("Time zone conversion is outside the supported calendar range", XmpError.BadValue, exception);
        }
    }

    /// <summary>Parses an XMP ISO 8601 value, including partial dates and times.</summary>
    public static XmpDateTime Parse(string value)
    {
        if (value is null) throw new XmpException("Parameter must not be null", XmpError.BadParameter);
        var result = new XmpDateTime();
        if (value.Length == 0) return result;
        var input = new ParseState(value);
        if (input.Current == '-') input.Advance();
        result.Year = input.Integer("Invalid year in date string", 9999);
        input.RequireDelimiter('-', "Invalid date string, after year");
        result.HasDate = true;
        if (input.End) return result;
        input.Advance();
        result.Month = Math.Max(1, input.Integer("Invalid month in date string", 12));
        input.RequireDelimiter('-', "Invalid date string, after month");
        if (input.End) return result;
        input.Advance();
        result.Day = Math.Max(1, input.Integer("Invalid day in date string", 31));
        input.RequireDelimiter('T', "Invalid date string, after day");
        if (input.End) return result;
        input.Advance();
        result.Hour = input.Integer("Invalid hour in date string", 23);
        result.HasTime = true;
        if (input.End) return result;
        if (input.Current == ':')
        {
            input.Advance();
            result.Minute = input.Integer("Invalid minute in date string", 59);
            input.RequireOneOf(":Z+-", "Invalid date string, after minute");
        }
        if (input.End) return result;
        if (input.Current == ':')
        {
            input.Advance();
            result.Second = input.Integer("Invalid whole seconds in date string", 59);
            input.RequireOneOf(".Z+-", "Invalid date string, after whole seconds");
            if (input.Current == '.')
            {
                input.Advance();
                var start = input.Position;
                var fraction = input.Integer("Invalid fractional seconds in date string", 999999999);
                input.RequireOneOf("Z+-", "Invalid date string, after fractional second");
                var digits = input.Position - start;
                while (digits > 9) { fraction /= 10; digits--; }
                while (digits < 9) { fraction *= 10; digits++; }
                result.Nanosecond = fraction;
            }
        }
        else
            input.RequireOneOf("Z+-", "Invalid date string, after time");

        if (input.End) return result;
        var sign = 0;
        var zoneHour = 0;
        var zoneMinute = 0;
        if (input.Current == 'Z')
            input.Advance();
        else
        {
            sign = input.Current == '+' ? 1 : -1;
            input.Advance();
            zoneHour = input.Integer("Invalid time zone hour in date string", 23);
            if (!input.End)
            {
                if (input.Current != ':') throw new XmpException("Invalid date string, after time zone hour", XmpError.BadValue);
                input.Advance();
                zoneMinute = input.Integer("Invalid time zone minute in date string", 59);
            }
        }
        if (!input.End) throw new XmpException("Invalid date string, extra chars at end", XmpError.BadValue);
        result.OffsetMinutes = (zoneHour * 60 + zoneMinute) * sign;
        return result;
    }

    /// <summary>Renders using the reference toolkit's canonical output conventions.</summary>
    public override string ToString()
    {
        if (!HasDate) return "";
        var text = new StringBuilder(Year.ToString("D4", CultureInfo.InvariantCulture));
        if (Month == 0) return text.ToString();
        text.Append('-').Append(Month.ToString("D2", CultureInfo.InvariantCulture));
        if (Day == 0) return text.ToString();
        text.Append('-').Append(Day.ToString("D2", CultureInfo.InvariantCulture));
        if (!HasTime) return text.ToString();
        text.Append('T').Append(Hour.ToString("D2", CultureInfo.InvariantCulture))
            .Append(':').Append(Minute.ToString("D2", CultureInfo.InvariantCulture));
        if (Second != 0 || Nanosecond != 0)
        {
            text.Append(':').Append(Second.ToString("D2", CultureInfo.InvariantCulture));
            if (Nanosecond != 0)
                text.Append('.').Append(Nanosecond.ToString("D9", CultureInfo.InvariantCulture).TrimEnd('0'));
        }
        if (OffsetMinutes is int offset)
        {
            if (offset == 0) text.Append('Z');
            else
            {
                // The Java renderer derives the sign from the truncated hour, including for -00:xx.
                var hours = offset / 60;
                text.Append(hours < 0 ? '-' : '+').Append(Math.Abs(hours).ToString("D2", CultureInfo.InvariantCulture))
                    .Append(':').Append(Math.Abs(offset % 60).ToString("D2", CultureInfo.InvariantCulture));
            }
        }
        return text.ToString();
    }

    private sealed class ParseState(string value)
    {
        public int Position { get; private set; }
        public bool End => Position >= value.Length;
        public char Current => End ? '\0' : value[Position];
        public void Advance() => Position++;

        public int Integer(string message, int maximum)
        {
            var start = Position;
            var number = 0;
            while (Current is >= '0' and <= '9')
            {
                number = unchecked(number * 10 + Current - '0');
                Position++;
            }
            if (Position == start) throw new XmpException(message, XmpError.BadValue);
            return number > maximum ? maximum : Math.Max(number, 0);
        }

        public void RequireDelimiter(char delimiter, string message)
        {
            if (!End && Current != delimiter) throw new XmpException(message, XmpError.BadValue);
        }

        public void RequireOneOf(string allowed, string message)
        {
            if (!End && !allowed.Contains(Current)) throw new XmpException(message, XmpError.BadValue);
        }
    }
}
