using System.ComponentModel;
using System.IO.Ports;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
    private readonly TerminalConfiguration configuration = TerminalConfiguration.LoadLast();
    private readonly DiagnosticLog log = new();
    private readonly TerminalSession session = new();
    private readonly List<string> commandHistory = [];
    private readonly List<ReceivedData> receivedData = [];
    private readonly object receivedDataLock = new();
    private readonly DateTime sessionStarted = DateTime.Now;
    private int commandHistoryIndex;

    public MainWindow()
    {
        DataContext = configuration; InitializeComponent();
        BaudBox.ItemsSource = new[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
        ParityBox.ItemsSource = Enum.GetValues<Parity>(); DataBitsBox.ItemsSource = new[] { 5, 6, 7, 8 }; StopBitsBox.ItemsSource = new[] { StopBits.One, StopBits.OnePointFive, StopBits.Two };
        HandshakeBox.ItemsSource = Enum.GetValues<Handshake>(); RtsBox.ItemsSource = Enum.GetValues<SerialDevice.RtsModes>(); ReceivedFormatBox.ItemsSource = new[] { ReceivedDataFormat.Text, ReceivedDataFormat.Bytes };
        configuration.PropertyChanged += Configuration_PropertyChanged; configuration.Changed += Configuration_Changed; log.EntryRecorded += AppendDiagnostic;
        session.Log = log; session.DataReceived += ReceivedDataReceived; session.Connected += (_, _) => PostConnectionState(); session.Disconnecting += (_, _) => PostConnectionState(false);
        RefreshPorts(); ApplyDiagnosticWordWrap(); UpdateConnectionState(false); UpdateSignalDisplay(null); UpdateLogFileDisplay(); Closed += MainWindow_Closed;
    }
    private void RefreshPorts_Click(object? sender, RoutedEventArgs e) => RefreshPorts();
    private void RefreshPorts() { var ports = SerialPort.GetPortNames().OrderBy(PortSortKey).ThenBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray(); PortBox.ItemsSource = ports; if (string.IsNullOrWhiteSpace(configuration.PortName)) configuration.PortName = ports.FirstOrDefault(); }
    private static int PortSortKey(string p) => p.StartsWith("COM", StringComparison.OrdinalIgnoreCase) && int.TryParse(p.AsSpan(3), out var n) ? n : int.MaxValue;
    private void Connect_Click(object? sender, RoutedEventArgs eventArgs)
    {
        if (session.Ready) { session.Disconnect(); UpdateConnectionState(false); UpdateSignalDisplay(null); return; }
        SerialPortSettings portSettings; try { portSettings = configuration.CreatePortSettings(); } catch (InvalidOperationException e) { AppendDiagnostic(e.Message); return; }
        if (!configuration.TryCreateCrcOptions(out var crc, out var crcError)) { AppendDiagnostic(crcError!); return; }
        EnsureSessionLog(); log.Record($"Connection configuration: {configuration.Describe()}"); session.SerialDevice?.Dispose();
        var device = new SerialDevice(portSettings) { RtsMode = configuration.RtsMode, CrcConfig = crc, IgnoreCRCErrors = configuration.DeliverCrcErrors, LogSignals = configuration.LogSignals }; device.SignalsChanged += SignalsChanged; session.SerialDevice = device;
        session.LogEverything = configuration.IncludeDetailedTransportEvents; session.LogCommands = true; session.LogResponses = true;
        if (!session.Connect()) log.Record($"Unable to connect to {portSettings.PortName}."); UpdateConnectionState(session.Ready); if (!session.Ready) UpdateSignalDisplay(null);
    }
    private void EnsureSessionLog()
    {
        if (log.FileName is not null) return; var directory = settings.ResolvedLogFolder; Directory.CreateDirectory(directory); log.FileName = Path.Combine(directory, $"{settings.LogFilePrefix}{sessionStarted:yyyy-MM-dd_HHmmss}.log");
        ApplyLogRetention(directory); UpdateLogFileDisplay(); log.Record($"Diagnostic session started. Log: {log.FileName}"); log.Record($"Initial configuration: {configuration.Describe()}");
    }
    private void ApplyLogRetention(string directory)
    {
        var current = log.FileName; var logs = Directory.EnumerateFiles(directory, settings.LogFilePrefix + "*.log", SearchOption.TopDirectoryOnly).Where(p => !string.Equals(Path.GetFullPath(p), current, StringComparison.OrdinalIgnoreCase)).OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var oldLog in logs.Skip(Math.Max(0, settings.LogFilesToKeep - 1))) try { File.Delete(oldLog); } catch { }
    }
    private void UpdateLogFileDisplay() { if (LogFileBox is not null) LogFileBox.Text = log.FileName is null ? "(starts on first connection attempt)" : Path.GetFileName(log.FileName); }
    private void Send_Click(object? sender, RoutedEventArgs e) => SendCommand();
    private void CommandBox_KeyDown(object? sender, KeyEventArgs e) { switch (e.Key) { case Key.Enter: e.Handled = true; SendCommand(); break; case Key.Up: e.Handled = RecallCommand(-1); break; case Key.Down: e.Handled = RecallCommand(1); break; } }
    private bool RecallCommand(int direction) { if (commandHistory.Count == 0) return false; commandHistoryIndex = Math.Clamp(commandHistoryIndex + direction, 0, commandHistory.Count); CommandBox.Text = commandHistoryIndex == commandHistory.Count ? string.Empty : commandHistory[commandHistoryIndex]; CommandBox.CaretIndex = CommandBox.Text?.Length ?? 0; return true; }
    private void SendCommand()
    {
        var expression = CommandBox.Text ?? string.Empty; if (expression.Length == 0) return;
        if (!SendDataParser.TryParse(expression, out var bytes, out var parseError)) { EnsureSessionLog(); log.Record($"Send rejected: {parseError}"); return; }
        if (!session.Ready) { EnsureSessionLog(); log.Record($"Send rejected: no device connected. Expression: \"{expression}\""); return; }
        if (session.Send(bytes)) { commandHistory.Add(expression); commandHistoryIndex = commandHistory.Count; CommandBox.SelectAll(); }
    }
    private void ReceivedDataReceived(ReceivedData data) { lock (receivedDataLock) receivedData.Add(data); AppendReceived(FormatReceivedData(data)); }
    private string FormatReceivedData(ReceivedData data)
    {
        var bytes = data.GetDisplayBytes(configuration.ShowCrcBytes);
        return configuration.ReceivedDataFormat == ReceivedDataFormat.Bytes ? SerialDataFormatter.ToByteString(bytes) : SerialDataFormatter.ToEscapedText(bytes);
    }
    private void RerenderReceivedData() { ReceivedData[] snapshot; lock (receivedDataLock) snapshot = receivedData.ToArray(); var rendered = string.Join(Environment.NewLine, snapshot.Select(FormatReceivedData)); Dispatcher.UIThread.Post(() => { ReceivedBox.Text = rendered; ReceivedBox.CaretIndex = ReceivedBox.Text?.Length ?? 0; }); }
    private void SignalsChanged(SerialSignalState state) => Dispatcher.UIThread.Post(() => UpdateSignalDisplay(state));
    private void UpdateSignalDisplay(SerialSignalState? state)
    {
        static string Show(string name, bool value) => $"{name} {(value ? "●" : "○")}";
        if (state is not { } s) { RtsSignal.Text = "RTS —"; CtsSignal.Text = "CTS —"; DtrSignal.Text = "DTR —"; DsrSignal.Text = "DSR —"; DcdSignal.Text = "DCD —"; RiSignal.Text = "RI —"; return; }
        RtsSignal.Text = Show("RTS", s.Rts); CtsSignal.Text = Show("CTS", s.Cts); DtrSignal.Text = Show("DTR", s.Dtr); DsrSignal.Text = Show("DSR", s.Dsr); DcdSignal.Text = Show("DCD", s.Dcd); RiSignal.Text = Show("RI", s.Ring);
    }
    private void Configuration_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TerminalConfiguration.DiagnosticWordWrap)) Dispatcher.UIThread.Post(ApplyDiagnosticWordWrap);
        else if (e.PropertyName == nameof(TerminalConfiguration.IncludeDetailedTransportEvents)) session.LogEverything = configuration.IncludeDetailedTransportEvents;
        else if (e.PropertyName == nameof(TerminalConfiguration.LogSignals) && session.SerialDevice is { } device) device.LogSignals = configuration.LogSignals;
        else if (e.PropertyName is nameof(TerminalConfiguration.ReceivedDataFormat) or nameof(TerminalConfiguration.ShowCrcBytes)) RerenderReceivedData();
    }
    private void Configuration_Changed(object? sender, ConfigurationChangedEventArgs e) { if (log.FileName is not null) log.Record($"Configuration changed: {e.PropertyName}: {FormatConfigurationValue(e.OldValue)} -> {FormatConfigurationValue(e.NewValue)}"); }
    private static string FormatConfigurationValue(object? value) => value switch { null => "(null)", string text => $"\"{text}\"", bool b => b ? "true" : "false", _ => value.ToString() ?? "(null)" };
    private void ApplyDiagnosticWordWrap() { DiagnosticsBox.TextWrapping = configuration.DiagnosticWordWrap ? Avalonia.Media.TextWrapping.Wrap : Avalonia.Media.TextWrapping.NoWrap; DiagnosticsBox.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, configuration.DiagnosticWordWrap ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto); }
    private void ClearReceived_Click(object? sender, RoutedEventArgs e) { lock (receivedDataLock) receivedData.Clear(); ReceivedBox.Clear(); }
    private void ClearDiagnostics_Click(object? sender, RoutedEventArgs e) => DiagnosticsBox.Clear();
    private void AppendReceived(string entry) => AppendText(ReceivedBox, entry, true); private void AppendDiagnostic(string entry) => AppendText(DiagnosticsBox, entry, false);
    private static void AppendText(TextBox box, string entry, bool separateEntry) { if (!Dispatcher.UIThread.CheckAccess()) { Dispatcher.UIThread.Post(() => AppendText(box, entry, separateEntry)); return; } var prefix = separateEntry && !string.IsNullOrEmpty(box.Text) ? Environment.NewLine : string.Empty; box.Text += prefix + entry; box.CaretIndex = box.Text?.Length ?? 0; }
    private void PostConnectionState(bool? connected = null) => Dispatcher.UIThread.Post(() => UpdateConnectionState(connected ?? session.Ready));
    private void UpdateConnectionState(bool connected) { ConnectionStatus.Text = connected ? $"Connected: {session.SerialDevice?.PortSettings.PortName}" : "Disconnected"; ConnectButton.Content = connected ? "Disconnect" : "Connect"; SendButton.IsEnabled = connected; PortBox.IsEnabled = !connected; BaudBox.IsEnabled = !connected; ParityBox.IsEnabled = !connected; DataBitsBox.IsEnabled = !connected; StopBitsBox.IsEnabled = !connected; HandshakeBox.IsEnabled = !connected; RtsBox.IsEnabled = !connected; RefreshPortsButton.IsEnabled = !connected; }
    private void MainWindow_Closed(object? sender, EventArgs e) { configuration.PropertyChanged -= Configuration_PropertyChanged; configuration.Changed -= Configuration_Changed; try { configuration.SaveLast(); } catch { } log.EntryRecorded -= AppendDiagnostic; session.Dispose(); log.Dispose(); }
}
