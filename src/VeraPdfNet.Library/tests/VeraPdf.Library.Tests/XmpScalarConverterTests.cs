using System.Globalization;
using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpScalarConverterTests
{
    [Fact]
    public void ScalarConversionsMatchJavaReference()
    {
        foreach (var line in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-scalar-reference.tsv")))
        {
            var fields = line.Split('\t');
            var input = Encoding.UTF8.GetString(Convert.FromBase64String(fields[1]));
            string ConvertValue() => fields[0] switch
            {
                "boolean" => XmpScalarConverter.ToBoolean(input).ToString().ToLowerInvariant(),
                "integer" => XmpScalarConverter.ToInteger(input).ToString(CultureInfo.InvariantCulture),
                "long" => XmpScalarConverter.ToLong(input).ToString(CultureInfo.InvariantCulture),
                "double" => unchecked((ulong)BitConverter.DoubleToInt64Bits(XmpScalarConverter.ToDouble(input)))
                    .ToString("x16", CultureInfo.InvariantCulture),
                "base64" => Convert.ToBase64String(XmpScalarConverter.DecodeBase64(input)),
                _ => throw new InvalidDataException("Unknown reference operation")
            };
            if (fields[2] == "ERR")
                Assert.Equal(int.Parse(fields[3], CultureInfo.InvariantCulture),
                    (int)Assert.Throws<XmpException>(ConvertValue).Error);
            else
                Assert.Equal(fields[3], ConvertValue());
        }
    }

    [Fact]
    public void ConvertsValuesWithoutDependingOnCurrentCulture()
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.True(XmpScalarConverter.ToBoolean("TRUE"));
            Assert.Equal("-1234567", XmpScalarConverter.FromLong(-1234567));
            Assert.Equal("42", XmpScalarConverter.FromInteger(42));
            Assert.Equal("True", XmpScalarConverter.FromBoolean(true));
            Assert.Equal("False", XmpScalarConverter.FromBoolean(false));
            Assert.Equal("0.1", XmpScalarConverter.FromDouble(0.1));
            Assert.Equal("1.0E7", XmpScalarConverter.FromDouble(10_000_000));
            Assert.Equal("0.001", XmpScalarConverter.FromDouble(0.001));
            Assert.Equal(1.25, XmpScalarConverter.ToDouble("1.25"));
        }
        finally { CultureInfo.CurrentCulture = old; }
    }

    [Fact]
    public void FormatsDoubleSpecialValuesAndPreservesNegativeZero()
    {
        Assert.Equal("NaN", XmpScalarConverter.FromDouble(double.NaN));
        Assert.Equal("Infinity", XmpScalarConverter.FromDouble(double.PositiveInfinity));
        Assert.Equal("-Infinity", XmpScalarConverter.FromDouble(double.NegativeInfinity));
        Assert.Equal("-0.0", XmpScalarConverter.FromDouble(BitConverter.Int64BitsToDouble(long.MinValue)));
        Assert.Equal("0.0", XmpScalarConverter.FromDouble(0));
    }

    [Fact]
    public void FiniteDoubleFormattingRoundTripsAtNotationBoundariesAndExtremes()
    {
        double[] values = [0.0001, 0.001, 1_000_000, 10_000_000, double.Epsilon, double.MaxValue, double.MinValue];
        foreach (var value in values)
        {
            var text = XmpScalarConverter.FromDouble(value);
            Assert.Equal(BitConverter.DoubleToInt64Bits(value),
                BitConverter.DoubleToInt64Bits(XmpScalarConverter.ToDouble(text)));
        }
    }

    [Fact]
    public void DateConversionRequiresNonemptyInput()
    {
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.ToDate("")).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.ToDate(null)).Error);
        Assert.Equal("2024-01", XmpScalarConverter.ToDate("2024-01").ToString());
        Assert.Equal("2024-01", XmpScalarConverter.FromDate(XmpScalarConverter.ToDate("2024-01")));
        Assert.Equal("2024-01-02T03:04:05.123Z",
            XmpScalarConverter.FromDate(XmpScalarConverter.ToDate("2024-01-02T03:04:05.123000Z")));
        Assert.Throws<ArgumentNullException>(() => XmpScalarConverter.FromDate(null!));
    }

    [Fact]
    public void Base64RoundTripsBinaryValues()
    {
        byte[] source = [0, 1, 127, 128, 255];
        Assert.Equal("AAF/gP8=", XmpScalarConverter.EncodeBase64(source));
        Assert.Equal(source, XmpScalarConverter.DecodeBase64(XmpScalarConverter.EncodeBase64(source)));
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.DecodeBase64(null)).Error);
    }

    [Fact]
    public void NullAndEmptyNumericInputsAreErrors()
    {
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.ToInteger(null)).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.ToLong("")).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.ToBoolean(null)).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.ToDouble(null)).Error);
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => XmpScalarConverter.ToDouble("")).Error);
    }

    [Fact]
    public void Base64RoundTripsVaryingLengths()
    {
        var random = new Random(5821);
        for (var length = 0; length < 128; length++)
        {
            var bytes = new byte[length];
            random.NextBytes(bytes);
            Assert.Equal(bytes, XmpScalarConverter.DecodeBase64(XmpScalarConverter.EncodeBase64(bytes)));
        }
    }
}
