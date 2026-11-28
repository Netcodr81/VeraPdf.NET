// Derived from veraPDF xmp-core, Copyright 2001-2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace VeraPdf.Library.Xmp;

/// <summary>Conversions for the scalar XMP property types implemented by this port.</summary>
public static class XmpScalarConverter
{
    private static readonly Regex DoubleSyntax = new(
        @"\A[+-]?(?:(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?|0[xX](?:[0-9a-fA-F]+(?:\.[0-9a-fA-F]*)?|\.[0-9a-fA-F]+)[pP][+-]?[0-9]+)[fFdD]?\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    public static bool ToBoolean(string? value)
    {
        RequireValue(value);
        if (TryParseInteger(value!, 10, int.MinValue, int.MaxValue, out var number))
            return number != 0;
        return value!.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("t", StringComparison.OrdinalIgnoreCase)
            || value.Equals("on", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    public static string FromBoolean(bool value) => value ? "True" : "False";

    public static int ToInteger(string? value)
    {
        RequireValue(value);
        var (digits, radix) = DigitsAndRadix(value!);
        if (!TryParseInteger(digits, radix, int.MinValue, int.MaxValue, out var result))
            throw new XmpException("Invalid integer string", XmpError.BadValue);
        return (int)result;
    }

    public static long ToLong(string? value)
    {
        RequireValue(value);
        var (digits, radix) = DigitsAndRadix(value!);
        if (!TryParseInteger(digits, radix, long.MinValue, long.MaxValue, out var result))
            throw new XmpException("Invalid long string", XmpError.BadValue);
        return result;
    }

    public static string FromInteger(int value) => value.ToString(CultureInfo.InvariantCulture);

    public static string FromLong(long value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Formats a double using invariant shortest-round-trip digits and Java-style exponent notation.</summary>
    public static string FromDouble(double value)
    {
        if (double.IsNaN(value)) return "NaN";
        if (double.IsPositiveInfinity(value)) return "Infinity";
        if (double.IsNegativeInfinity(value)) return "-Infinity";
        if (value == 0)
            return BitConverter.DoubleToInt64Bits(value) < 0 ? "-0.0" : "0.0";

        var text = Math.Abs(value).ToString("R", CultureInfo.InvariantCulture);
        var exponentIndex = text.IndexOfAny(['e', 'E']);
        var exponent = exponentIndex < 0
            ? 0
            : int.Parse(text.AsSpan(exponentIndex + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        var significand = exponentIndex < 0 ? text : text[..exponentIndex];
        var decimalPoint = significand.IndexOf('.');
        if (decimalPoint < 0) decimalPoint = significand.Length;
        var digits = significand.Replace(".", "", StringComparison.Ordinal);
        var firstDigit = 0;
        while (digits[firstDigit] == '0') firstDigit++;
        exponent += decimalPoint - firstDigit - 1;
        digits = digits[firstDigit..].TrimEnd('0');
        if (digits.Length == 0) digits = "0";

        var sign = value < 0 ? "-" : "";
        if (exponent is >= -3 and < 7)
        {
            var decimalPosition = exponent + 1;
            if (decimalPosition <= 0)
                return sign + "0." + new string('0', -decimalPosition) + digits;
            if (decimalPosition >= digits.Length)
                return sign + digits + new string('0', decimalPosition - digits.Length) + ".0";
            return sign + digits[..decimalPosition] + "." + digits[decimalPosition..];
        }

        var fraction = digits.Length == 1 ? "0" : digits[1..];
        return sign + digits[0] + "." + fraction + "E" + exponent.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Parses Java-compatible decimal and hexadecimal floating-point strings.</summary>
    public static double ToDouble(string? value)
    {
        RequireValue(value);
        var start = 0;
        var end = value!.Length;
        while (start < end && value[start] <= ' ') start++;
        while (end > start && value[end - 1] <= ' ') end--;
        var text = value[start..end];
        var unsigned = text.AsSpan();
        if (!unsigned.IsEmpty && unsigned[0] is '+' or '-') unsigned = unsigned[1..];
        if (unsigned.SequenceEqual("NaN")) return BitConverter.Int64BitsToDouble(0x7ff8000000000000);
        if (unsigned.SequenceEqual("Infinity")) return text.StartsWith('-') ? double.NegativeInfinity : double.PositiveInfinity;
        if (!DoubleSyntax.IsMatch(text))
            throw new XmpException("Invalid double string", XmpError.BadValue);

        if (text.EndsWith('f') || text.EndsWith('F') || text.EndsWith('d') || text.EndsWith('D'))
            text = text[..^1];
        unsigned = text.AsSpan();
        if (unsigned[0] is '+' or '-') unsigned = unsigned[1..];
        if (unsigned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return ParseHexDouble(text);
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            throw new XmpException("Invalid double string", XmpError.BadValue);
        return result;
    }

    private static double ParseHexDouble(string text)
    {
        var negative = text[0] == '-';
        var start = text[0] is '+' or '-' ? 3 : 2;
        var exponentIndex = text.IndexOfAny(['p', 'P'], start);
        var significand = text[start..exponentIndex];
        var point = significand.IndexOf('.');
        var fractionDigits = point < 0 ? 0 : significand.Length - point - 1;
        var digits = significand.Replace(".", "", StringComparison.Ordinal);
        var mantissa = BigInteger.Parse("0" + digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

        var exponentText = text.AsSpan(exponentIndex + 1);
        var exponentNegative = exponentText[0] == '-';
        if (exponentText[0] is '+' or '-') exponentText = exponentText[1..];
        long exponent = 0;
        foreach (var character in exponentText)
            exponent = Math.Min(10_000_000_000L, exponent * 10 + character - '0');
        if (exponentNegative) exponent = -exponent;
        exponent -= 4L * fractionDigits;

        var bits = mantissa.GetBitLength();
        ulong magnitude = 0;
        if (bits != 0)
        {
            var binaryExponent = bits - 1 + exponent;
            if (binaryExponent > 1023) magnitude = 0x7ff0000000000000;
            else if (binaryExponent >= -1075)
            {
                var shift = binaryExponent >= -1022 ? bits - 53 : -1074 - exponent;
                BigInteger rounded;
                if (shift > 0)
                {
                    if (shift > bits) rounded = BigInteger.Zero;
                    else
                    {
                        var count = checked((int)shift);
                        rounded = mantissa >> count;
                        var remainder = mantissa - (rounded << count);
                        var halfway = BigInteger.One << (count - 1);
                        if (remainder > halfway || (remainder == halfway && !rounded.IsEven)) rounded++;
                    }
                }
                else rounded = mantissa << checked((int)-shift);

                if (binaryExponent < -1022) magnitude = (ulong)rounded;
                else
                {
                    if (rounded.GetBitLength() > 53) { rounded >>= 1; binaryExponent++; }
                    magnitude = binaryExponent > 1023 ? 0x7ff0000000000000
                        : ((ulong)(binaryExponent + 1023) << 52) | ((ulong)rounded & 0x000fffffffffffff);
                }
            }
        }
        if (negative) magnitude |= 0x8000000000000000;
        return BitConverter.Int64BitsToDouble(unchecked((long)magnitude));
    }

    public static XmpDateTime ToDate(string? value)
    {
        RequireValue(value);
        return XmpDateTime.Parse(value!);
    }

    public static string FromDate(XmpDateTime value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ToString();
    }

    public static string EncodeBase64(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Convert.ToBase64String(value);
    }

    /// <summary>
    /// Decodes using the reference's permissive treatment of missing and misplaced padding.
    /// </summary>
    public static byte[] DecodeBase64(string? value)
    {
        if (value is null) throw new XmpException("Invalid base64 string", XmpError.BadValue);
        var sextets = new List<byte>(value.Length);
        foreach (var character in value)
        {
            if (character is ' ' or '\t' or '\r' or '\n' or '=') continue;
            var digit = character switch
            {
                >= 'A' and <= 'Z' => character - 'A',
                >= 'a' and <= 'z' => character - 'a' + 26,
                >= '0' and <= '9' => character - '0' + 52,
                '+' => 62,
                '/' => 63,
                _ => -1
            };
            if (digit < 0) throw new XmpException("Invalid base64 string", XmpError.BadValue);
            sextets.Add((byte)digit);
        }

        var result = new byte[(int)((long)sextets.Count * 3 / 4)];
        var buffer = 0;
        var bits = 0;
        var index = 0;
        foreach (var digit in sextets)
        {
            buffer = (buffer << 6) | digit;
            bits += 6;
            if (bits >= 8)
            {
                bits -= 8;
                result[index++] = (byte)(buffer >> bits);
            }
        }
        return result;
    }

    private static void RequireValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new XmpException("Empty convert-string", XmpError.BadValue);
    }

    private static (string Digits, int Radix) DigitsAndRadix(string value) =>
        value.StartsWith("0x", StringComparison.Ordinal) ? (value[2..], 16) : (value, 10);

    private static bool TryParseInteger(string text, int radix, long minimum, long maximum, out long value)
    {
        value = 0;
        if (text.Length == 0) return false;
        var negative = text[0] == '-';
        var index = text[0] is '-' or '+' ? 1 : 0;
        if (index == text.Length) return false;
        var limit = negative ? minimum : -maximum;
        var multiplyLimit = limit / radix;
        long result = 0;
        for (; index < text.Length; index++)
        {
            var digit = Digit(text[index], radix);
            if (digit < 0 || result < multiplyLimit) return false;
            result *= radix;
            if (result < limit + digit) return false;
            result -= digit;
        }
        value = negative ? result : -result;
        return true;
    }

    private static int Digit(char value, int radix)
    {
        var digit = CharUnicodeInfo.GetDecimalDigitValue(value);
        if (digit < 0 && radix == 16)
        {
            digit = value switch
            {
                >= 'a' and <= 'f' => value - 'a' + 10,
                >= 'A' and <= 'F' => value - 'A' + 10,
                >= '\uff41' and <= '\uff46' => value - '\uff41' + 10,
                >= '\uff21' and <= '\uff26' => value - '\uff21' + 10,
                _ => -1
            };
        }
        return digit < radix ? digit : -1;
    }
}
