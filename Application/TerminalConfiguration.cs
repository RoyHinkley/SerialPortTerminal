using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using SerialPortTerminal.Serial;

namespace SerialPortTerminal.Application;

public enum ReceivedDataFormat
{
    Text,
    EscapedText,
    Bytes
}

/// <summary>
/// Mutable application state edited by the UI and consumed by the terminal/serial layers.
/// The view binds to this model; controls are not the authoritative configuration store.
/// </summary>
public sealed class TerminalConfiguration : INotifyPropertyChanged
{
    private string? portName;
    private int baudRate = 115200;
    private Parity parity = Parity.None;
    private int dataBits = 8;
    private StopBits stopBits = StopBits.One;
    private Handshake handshake = Handshake.None;
    private SerialDevice.RtsModes rtsMode = SerialDevice.RtsModes.Enabled;
    private bool useCrc;
    private string crcPolynomial = "DAAE";
    private string crcInitialValue = "FFFF";
    private string crcExpectedResidue = "82C0";
    private string termChar = "03";
    private bool crcPostInvert = true;
    private bool crcMsBitFirst;
    private bool crcMsByteFirst;
    private bool omitTermChar;
    private bool deliverCrcErrors;
    private bool includeDetailedTransportEvents = true;
    private ReceivedDataFormat receivedDataFormat;
    private bool showCrcBytes;
    private bool diagnosticWordWrap;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? PortName { get => portName; set => Set(ref portName, value); }
    public int BaudRate { get => baudRate; set => Set(ref baudRate, value); }
    public Parity Parity { get => parity; set => Set(ref parity, value); }
    public int DataBits { get => dataBits; set => Set(ref dataBits, value); }
    public StopBits StopBits { get => stopBits; set => Set(ref stopBits, value); }
    public Handshake Handshake { get => handshake; set => Set(ref handshake, value); }
    public SerialDevice.RtsModes RtsMode { get => rtsMode; set => Set(ref rtsMode, value); }

    public bool UseCrc { get => useCrc; set => Set(ref useCrc, value); }
    public string CrcPolynomial { get => crcPolynomial; set => Set(ref crcPolynomial, value); }
    public string CrcInitialValue { get => crcInitialValue; set => Set(ref crcInitialValue, value); }
    public string CrcExpectedResidue { get => crcExpectedResidue; set => Set(ref crcExpectedResidue, value); }
    public string TermChar { get => termChar; set => Set(ref termChar, value); }
    public bool CrcPostInvert { get => crcPostInvert; set => Set(ref crcPostInvert, value); }
    public bool CrcMsBitFirst { get => crcMsBitFirst; set => Set(ref crcMsBitFirst, value); }
    public bool CrcMsByteFirst { get => crcMsByteFirst; set => Set(ref crcMsByteFirst, value); }
    public bool OmitTermChar { get => omitTermChar; set => Set(ref omitTermChar, value); }
    public bool DeliverCrcErrors { get => deliverCrcErrors; set => Set(ref deliverCrcErrors, value); }

    public bool IncludeDetailedTransportEvents { get => includeDetailedTransportEvents; set => Set(ref includeDetailedTransportEvents, value); }
    public ReceivedDataFormat ReceivedDataFormat { get => receivedDataFormat; set => Set(ref receivedDataFormat, value); }
    public bool ShowCrcBytes { get => showCrcBytes; set => Set(ref showCrcBytes, value); }
    public bool DiagnosticWordWrap { get => diagnosticWordWrap; set => Set(ref diagnosticWordWrap, value); }

    public SerialPortSettings CreatePortSettings()
    {
        if (string.IsNullOrWhiteSpace(PortName))
            throw new InvalidOperationException("No serial port is selected.");
        return new SerialPortSettings(PortName, BaudRate, Parity, DataBits, StopBits, Handshake);
    }

    public bool TryCreateCrcOptions(out CrcOptions? options, out string? error)
    {
        options = null;
        error = null;
        if (!UseCrc)
            return true;

        if (!TryHex16(CrcPolynomial, "CRC polynomial", out var polynomial, out error) ||
            !TryHex16(CrcInitialValue, "CRC initial value", out var initial, out error) ||
            !TryHex16(CrcExpectedResidue, "CRC expected residue", out var residue, out error) ||
            !TryHex8(TermChar, "termination character", out var term, out error))
            return false;

        options = new CrcOptions(initial, polynomial, residue, CrcPostInvert,
            CrcMsBitFirst, CrcMsByteFirst, term, OmitTermChar);
        return true;
    }

    private static bool TryHex16(string text, string name, out ushort value, out string? error)
    {
        if (ushort.TryParse(NormalizeHex(text), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
        {
            error = null;
            return true;
        }
        error = $"Invalid {name}: '{text}'. Enter 1-4 hexadecimal digits.";
        return false;
    }

    private static bool TryHex8(string text, string name, out byte value, out string? error)
    {
        if (byte.TryParse(NormalizeHex(text), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
        {
            error = null;
            return true;
        }
        error = $"Invalid {name}: '{text}'. Enter 1-2 hexadecimal digits.";
        return false;
    }

    private static string NormalizeHex(string? text)
    {
        var value = text?.Trim() ?? string.Empty;
        return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
