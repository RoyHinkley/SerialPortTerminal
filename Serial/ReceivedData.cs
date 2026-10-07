using System.Text;

namespace SerialPortTerminal.Serial;

/// <summary>
/// Immutable data retained for one receive unit recognized by <see cref="SerialDevice"/>.
/// </summary>
/// <remarks>
/// <see cref="WireBytes"/> preserves the bytes associated with the receive unit before presentation
/// removes CRC bytes. <see cref="PayloadBytes"/> is the application payload that would historically
/// have been forwarded as a string. Keeping both prevents display policy from destroying diagnostic
/// evidence and allows CRC acceptance to remain independent of whether CRC bytes are shown.
/// </remarks>
public sealed class ReceivedData
{
    private static readonly Encoding Latin1 = Encoding.Latin1;

    public ReceivedData(byte[] wireBytes, byte[] payloadBytes, bool? crcValid)
    {
        ArgumentNullException.ThrowIfNull(wireBytes);
        ArgumentNullException.ThrowIfNull(payloadBytes);
        WireBytes = Array.AsReadOnly((byte[])wireBytes.Clone());
        PayloadBytes = Array.AsReadOnly((byte[])payloadBytes.Clone());
        CrcValid = crcValid;
    }

    /// <summary>Bytes retained from the recognized receive unit, including CRC bytes when present.</summary>
    public IReadOnlyList<byte> WireBytes { get; }

    /// <summary>Application payload after protocol CRC bytes have been removed.</summary>
    public IReadOnlyList<byte> PayloadBytes { get; }

    /// <summary>True/false when CRC was checked; null when CRC was not configured.</summary>
    public bool? CrcValid { get; }

    public string PayloadText => Latin1.GetString(PayloadBytes.ToArray());

    public byte[] GetDisplayBytes(bool includeCrcBytes) =>
        (includeCrcBytes ? WireBytes : PayloadBytes).ToArray();
}
