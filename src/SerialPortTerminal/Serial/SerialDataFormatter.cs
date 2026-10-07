using System.Text;
using System.Text.RegularExpressions;

namespace SerialPortTerminal.Serial;

/// <summary>
/// Formats serial data for diagnostic display and logging without changing the underlying bytes.
/// </summary>
public static class SerialDataFormatter
{
    /// <summary>
    /// Formats each character as its low eight-bit hexadecimal byte value.
    /// </summary>
    public static string ToByteString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0)
            return string.Empty;

        var result = new StringBuilder(value.Length * 3 - 1);
        for (var i = 0; i < value.Length; i++)
        {
            if (i > 0)
                result.Append(' ');
            result.Append(((byte)value[i]).ToString("X2"));
        }

        return result.ToString();
    }

    public static string Format(string? value, bool binaryComms, bool escapeLoggedData)
    {
        value ??= string.Empty;
        if (binaryComms)
            return ToByteString(value);

        return escapeLoggedData ? Regex.Escape(value) : value;
    }

    /// <summary>
    /// Formats a byte buffer directly so CRC and other non-text bytes remain visible.
    /// </summary>
    public static string ToByteString(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return string.Empty;

        var result = new StringBuilder(bytes.Length * 3 - 1);
        for (var i = 0; i < bytes.Length; i++)
        {
            if (i > 0)
                result.Append(' ');
            result.Append(bytes[i].ToString("X2"));
        }

        return result.ToString();
    }
}
