using System.ComponentModel;
using System.IO.Ports;
using System.Text;
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
    private readonly TerminalConfiguration configuration = new();
    private readonly DiagnosticLog log = new();
    private readonly TerminalSession session = new();
    private readonly List<string> commandHistory = [];
    private readonly DateTime sessionStarted = DateTime.Now;
    private int commandHistoryIndex;

    public MainWindow()
    {
        DataContext = configuration;
        InitializeComponent();

        BaudBox.ItemsSource = new[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
        ParityBox.ItemsSource = Enum.GetValues<Parity>();
        DataBitsBox.ItemsSource = new[] { 5, 6, 7, 8 };
        StopBitsBox.ItemsSource = new[] { StopBits.One, StopBits.OnePointFive, StopBits.Two };
        HandshakeBox.ItemsSource = Enum.GetValues<Handshake>();
        RtsBox.ItemsSource = Enum.GetValues<SerialDevice.RtsModes>();
        ReceivedFormatBox.ItemsSource = Enum.GetValues<ReceivedDataFormat>();

        configuration.PropertyChanged += Configuration_PropertyChanged;
        log.EntryRecorded += AppendDiagnostic;
        session.Log = log;
        session.DataReceived += ReceivedDataReceived;
        session.Connected += (_, _) => PostConnectionState();
        session.Disconnecting += (_, _) => PostConnectionState(false);

        RefreshPorts();
        ApplyDiagnosticWordWrap();
        UpdateConnectionState(false);
        UpdateLogFileDisplay();
        Closed += MainWindow_Closed;
    }

    private void RefreshPorts_Click(object? sender, RoutedEventArgs e) => RefreshPorts();

    private void RefreshPorts()
    {
        var ports = SerialPort.GetPortNames()
            .OrderBy(PortSortKey)
            .ThenBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        PortBox.ItemsSource = ports;
        if (!ports.Contains(configuration.PortName, StringComparer.OrdinalIgnoreCase))
            configuration.PortName = ports.FirstOrDefault();
    }

    private static int PortSortKey(string portName)
    {
        if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(portName.AsSpan(3), out var number))
            return number;
        return int.MaxValue;
    }

    private void Connect_Click(object? sender, RoutedEventArgs eventArgs)
    {
        if (session.Ready)
        {
            session.Disconnect();
            UpdateConnectionState(false);
            return;
        }

        SerialPortSettings portSettings;
        try
        {
            portSettings = configuration.CreatePortSettings();
        }
        catch (InvalidOperationException exception)
        {
            AppendDiagnostic(exception.Message);
            return;
        }

        if (!configuration.TryCreateCrcOptions(out var crc, out var crcError))
        {
            AppendDiagnostic(crcError!);
            return;
        }

        EnsureSessionLog();

        session.SerialDevice?.Dispose();
        session.SerialDevice = new SerialDevice(portSettings)
        {
            RtsMode = configuration.RtsMode,
            CrcConfig = crc,
            IgnoreCRCErrors = configuration.DeliverCrcErrors
        };
        session.LogEverything = configuration.IncludeDetailedTransportEvents;
        session.LogCommands = true;
        session.LogResponses = true;

        if (!session.Connect())
            log.Record($"Unable to connect to {portSettings.PortName}.");
        UpdateConnectionState(session.Ready);
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
            catch { }
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

    private void ReceivedDataReceived(ReceivedData data)
    {
        // Received presentation is determined from application state, not Avalonia controls. The
        // serial processing thread therefore never needs to touch an object owned by the UI thread.
        var bytes = data.GetDisplayBytes(configuration.ShowCrcBytes);
        var formatted = configuration.ReceivedDataFormat switch
        {
            ReceivedDataFormat.Bytes => SerialDataFormatter.ToByteString(bytes),
            ReceivedDataFormat.EscapedText => SerialDataFormatter.Format(Encoding.Latin1.GetString(bytes), false, true),
            _ => Encoding.Latin1.GetString(bytes)
        };
        AppendReceived(formatted);
    }

    private void Configuration_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TerminalConfiguration.DiagnosticWordWrap))
            Dispatcher.UIThread.Post(ApplyDiagnosticWordWrap);
        else if (e.PropertyName == nameof(TerminalConfiguration.IncludeDetailedTransportEvents))
            session.LogEverything = configuration.IncludeDetailedTransportEvents;
    }

    private void ApplyDiagnosticWordWrap()
    {
        DiagnosticsBox.TextWrapping = configuration.DiagnosticWordWrap
            ? Avalonia.Media.TextWrapping.Wrap
            : Avalonia.Media.TextWrapping.NoWrap;
        DiagnosticsBox.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty,
            configuration.DiagnosticWordWrap ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto);
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
        configuration.PropertyChanged -= Configuration_PropertyChanged;
        log.EntryRecorded -= AppendDiagnostic;
        session.Dispose();
        log.Dispose();
    }
}
