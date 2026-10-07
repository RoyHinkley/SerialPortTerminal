using System.Globalization;
using System.IO.Ports;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SerialPortTerminal.Application;
using SerialPortTerminal.Diagnostics;
using SerialPortTerminal.Serial;

namespace SerialPortTerminal;

public sealed partial class MainWindow : Window
{
    private readonly AppSettings settings = AppSettings.Load();
    private readonly DiagnosticLog log = new();
    private readonly TerminalSession session = new();
    private readonly List<string> commandHistory = [];
    private readonly DateTime sessionStarted = DateTime.Now;
    private int commandHistoryIndex;

    public MainWindow()
    {
        InitializeComponent();

        BaudBox.ItemsSource = new[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
        BaudBox.SelectedItem = 115200;
        ParityBox.ItemsSource = Enum.GetValues<Parity>();
        ParityBox.SelectedItem = Parity.None;
        DataBitsBox.ItemsSource = new[] { 5, 6, 7, 8 };
        DataBitsBox.SelectedItem = 8;
        StopBitsBox.ItemsSource = new[] { StopBits.One, StopBits.OnePointFive, StopBits.Two };
        StopBitsBox.SelectedItem = StopBits.One;
        HandshakeBox.ItemsSource = Enum.GetValues<Handshake>();
        HandshakeBox.SelectedItem = Handshake.None;
        RtsBox.ItemsSource = Enum.GetValues<SerialDevice.RtsModes>();
        RtsBox.SelectedItem = SerialDevice.RtsModes.Enabled;

        log.EntryRecorded += AppendDiagnostic;
        session.Log = log;
        session.ResponseReceived += ResponseReceived;
        session.Connected += (_, _) => PostConnectionState();
        session.Disconnecting += (_, _) => PostConnectionState(false);

        RefreshPorts();
        UpdateConnectionState(false);
        UpdateLogFileDisplay();
        Closed += MainWindow_Closed;
    }

    private void RefreshPorts_Click(object? sender, RoutedEventArgs e) => RefreshPorts();

    private void RefreshPorts()
    {
        var selected = PortBox.SelectedItem as string;
        var ports = SerialPort.GetPortNames()
            .OrderBy(PortSortKey)
            .ThenBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        PortBox.ItemsSource = ports;
        PortBox.SelectedItem = ports.Contains(selected, StringComparer.OrdinalIgnoreCase)
            ? selected
            : ports.FirstOrDefault();
    }

    private static int PortSortKey(string portName)
    {
        if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(portName.AsSpan(3), out var number))
            return number;
        return int.MaxValue;
    }

    private void Connect_Click(object? sender, RoutedEventArgs e)
    {
        if (session.Ready)
        {
            session.Disconnect();
            UpdateConnectionState(false);
            return;
        }

        if (PortBox.SelectedItem is not string portName)
        {
            AppendDiagnostic("No serial port is selected.");
            return;
        }

        CrcOptions? crc = null;
        if (CrcBox.IsChecked == true && !TryGetCrcOptions(out crc))
            return;

        EnsureSessionLog();

        session.SerialDevice?.Dispose();
        session.SerialDevice = new SerialDevice(new SerialPortSettings(
            portName,
            BaudBox.SelectedItem is int baud ? baud : 115200,
            ParityBox.SelectedItem is Parity parity ? parity : Parity.None,
            DataBitsBox.SelectedItem is int dataBits ? dataBits : 8,
            StopBitsBox.SelectedItem is StopBits stopBits ? stopBits : StopBits.One,
            HandshakeBox.SelectedItem is Handshake handshake ? handshake : Handshake.None))
        {
            RtsMode = RtsBox.SelectedItem is SerialDevice.RtsModes rts ? rts : SerialDevice.RtsModes.Enabled,
            CrcConfig = crc,
            IgnoreCRCErrors = IgnoreCrcErrorsBox.IsChecked == true
        };
        session.LogEverything = VerboseBox.IsChecked == true;
        session.LogCommands = true;
        session.LogResponses = true;

        if (!session.Connect())
            log.Record($"Unable to connect to {portName}.");
        UpdateConnectionState(session.Ready);
    }

    private bool TryGetCrcOptions(out CrcOptions? options)
    {
        options = null;
        if (!TryHex16(CrcPolynomialBox.Text, "CRC polynomial", out var polynomial) ||
            !TryHex16(CrcInitialBox.Text, "CRC initial value", out var initial) ||
            !TryHex16(CrcResidueBox.Text, "CRC expected residue", out var residue) ||
            !TryHex8(TermCharBox.Text, "termination character", out var termChar))
            return false;

        options = new CrcOptions(
            initial,
            polynomial,
            residue,
            CrcPostInvertBox.IsChecked == true,
            CrcMsBitFirstBox.IsChecked == true,
            CrcMsByteFirstBox.IsChecked == true,
            termChar,
            OmitTermCharBox.IsChecked == true);
        return true;
    }

    private bool TryHex16(string? text, string name, out ushort value)
    {
        if (ushort.TryParse(NormalizeHex(text), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            return true;
        AppendDiagnostic($"Invalid {name}: '{text}'. Enter 1-4 hexadecimal digits.");
        return false;
    }

    private bool TryHex8(string? text, string name, out byte value)
    {
        if (byte.TryParse(NormalizeHex(text), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            return true;
        AppendDiagnostic($"Invalid {name}: '{text}'. Enter 1-2 hexadecimal digits.");
        return false;
    }

    private static string NormalizeHex(string? text)
    {
        var value = text?.Trim() ?? string.Empty;
        return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;
    }

    private void EnsureSessionLog()
    {
        if (log.FileName is not null)
            return;

        var directory = settings.ResolvedLogFolder;
        Directory.CreateDirectory(directory);
        var fileName = $"{settings.LogFilePrefix}{sessionStarted:yyyy-MM-dd_HHmmss}.log";
        log.FileName = Path.Combine(directory, fileName);
        ApplyLogRetention(directory);
        UpdateLogFileDisplay();
        log.Record($"Diagnostic session started. Log: {log.FileName}");
    }

    private void ApplyLogRetention(string directory)
    {
        var current = log.FileName;
        var pattern = settings.LogFilePrefix + "*.log";
        var automaticLogs = Directory.EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly)
            .Where(path => !string.Equals(Path.GetFullPath(path), current, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var oldLogs = automaticLogs.Skip(Math.Max(0, settings.LogFilesToKeep - 1));
        foreach (var oldLog in oldLogs)
        {
            try { File.Delete(oldLog); }
            catch { /* Retention failure is non-fatal and must not prevent communications. */ }
        }
    }

    private void UpdateLogFileDisplay()
    {
        if (LogFileBox is null)
            return;
        LogFileBox.Text = log.FileName is null ? "(starts on first connection attempt)" : Path.GetFileName(log.FileName);
    }

    private void Send_Click(object? sender, RoutedEventArgs e) => SendCommand();

    private void CommandBox_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;
                SendCommand();
                break;
            case Key.Up:
                e.Handled = RecallCommand(-1);
                break;
            case Key.Down:
                e.Handled = RecallCommand(1);
                break;
        }
    }

    private bool RecallCommand(int direction)
    {
        if (commandHistory.Count == 0)
            return false;

        commandHistoryIndex = Math.Clamp(commandHistoryIndex + direction, 0, commandHistory.Count);
        CommandBox.Text = commandHistoryIndex == commandHistory.Count
            ? string.Empty
            : commandHistory[commandHistoryIndex];
        CommandBox.CaretIndex = CommandBox.Text?.Length ?? 0;
        return true;
    }

    private void SendCommand()
    {
        var command = CommandBox.Text ?? string.Empty;
        if (command.Length == 0)
            return;
        if (!session.Ready)
        {
            AppendDiagnostic("Not connected.");
            return;
        }

        if (session.Send(command))
        {
            commandHistory.Add(command);
            commandHistoryIndex = commandHistory.Count;
            CommandBox.SelectAll();
        }
    }

    private void ResponseReceived(string response)
    {
        var formatted = BinaryBox.IsChecked == true
            ? SerialDataFormatter.ToByteString(response)
            : SerialDataFormatter.Format(response, false, EscapeBox.IsChecked == true);
        AppendReceived(formatted);
    }

    private void ClearReceived_Click(object? sender, RoutedEventArgs e) => ReceivedBox.Clear();
    private void ClearDiagnostics_Click(object? sender, RoutedEventArgs e) => DiagnosticsBox.Clear();

    private void AppendReceived(string entry) => AppendText(ReceivedBox, entry, separateEntry: true);
    private void AppendDiagnostic(string entry) => AppendText(DiagnosticsBox, entry, separateEntry: false);

    private static void AppendText(TextBox box, string entry, bool separateEntry)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => AppendText(box, entry, separateEntry));
            return;
        }

        var prefix = separateEntry && !string.IsNullOrEmpty(box.Text) ? Environment.NewLine : string.Empty;
        box.Text += prefix + entry;
        box.CaretIndex = box.Text?.Length ?? 0;
    }

    private void PostConnectionState(bool? connected = null) =>
        Dispatcher.UIThread.Post(() => UpdateConnectionState(connected ?? session.Ready));

    private void UpdateConnectionState(bool connected)
    {
        ConnectionStatus.Text = connected ? $"Connected: {session.SerialDevice?.PortSettings.PortName}" : "Disconnected";
        ConnectButton.Content = connected ? "Disconnect" : "Connect";
        SendButton.IsEnabled = connected;
        PortBox.IsEnabled = !connected;
        BaudBox.IsEnabled = !connected;
        ParityBox.IsEnabled = !connected;
        DataBitsBox.IsEnabled = !connected;
        StopBitsBox.IsEnabled = !connected;
        HandshakeBox.IsEnabled = !connected;
        RtsBox.IsEnabled = !connected;
        RefreshPortsButton.IsEnabled = !connected;
        CrcBox.IsEnabled = !connected;
        CrcPolynomialBox.IsEnabled = !connected;
        CrcInitialBox.IsEnabled = !connected;
        CrcResidueBox.IsEnabled = !connected;
        TermCharBox.IsEnabled = !connected;
        CrcPostInvertBox.IsEnabled = !connected;
        CrcMsBitFirstBox.IsEnabled = !connected;
        CrcMsByteFirstBox.IsEnabled = !connected;
        OmitTermCharBox.IsEnabled = !connected;
        IgnoreCrcErrorsBox.IsEnabled = !connected;
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        log.EntryRecorded -= AppendDiagnostic;
        session.Dispose();
        log.Dispose();
    }
}
