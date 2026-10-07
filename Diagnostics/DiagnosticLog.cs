using System.Text;

namespace SerialPortTerminal.Diagnostics;

/// <summary>
/// Thread-safe diagnostic log used to observe serial communications and transport behavior.
/// </summary>
/// <remarks>
/// SerialPortTerminal is a communications troubleshooting tool, so logging is part of its
/// functional behavior rather than merely a developer aid. Entries are exposed immediately
/// to observers and may also be persisted to a file.
/// </remarks>
public sealed class DiagnosticLog : IDisposable
{
    private readonly object sync = new();
    private StreamWriter? writer;
    private string? fileName;

    /// <summary>
    /// Raised synchronously after an entry has been accepted by the log.
    /// </summary>
    /// <remarks>
    /// Handlers run on the calling thread. UI subscribers must marshal to their UI thread.
    /// Subscriber exceptions are isolated from the communications code that produced the entry.
    /// </remarks>
    public event Action<string>? EntryRecorded;

    /// <summary>
    /// Gets or sets the file receiving persistent diagnostic output. Set to null or empty to disable file output.
    /// </summary>
    public string? FileName
    {
        get
        {
            lock (sync)
                return fileName;
        }
        set
        {
            lock (sync)
            {
                if (string.Equals(fileName, value, StringComparison.Ordinal))
                    return;

                CloseWriter();
                fileName = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }
    }

    public string TimeStampFormat { get; set; } = "MM/dd/yyyy HH:mm:ss.fff\t";

    public string TimeStamp() => DateTime.Now.ToString(TimeStampFormat);

    /// <summary>
    /// Records a timestamped diagnostic entry and flushes file output so evidence survives a crash or unplug event.
    /// </summary>
    public void Record(string entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var line = TimeStamp() + entry;

        lock (sync)
        {
            if (fileName is not null)
            {
                EnsureWriter();
                writer!.WriteLine(line);
                writer.Flush();
            }
        }

        DispatchEntry(line);
    }

    /// <summary>
    /// Writes text without adding a timestamp or newline.
    /// </summary>
    public void Write(string entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (sync)
        {
            if (fileName is not null)
            {
                EnsureWriter();
                writer!.Write(entry);
                writer.Flush();
            }
        }

        DispatchEntry(entry);
    }

    public void WriteLine(string entry) => Write(entry + Environment.NewLine);

    public void Close()
    {
        lock (sync)
            CloseWriter();
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }

    private void EnsureWriter()
    {
        if (writer is not null || fileName is null)
            return;

        var fullPath = Path.GetFullPath(fileName);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        writer = new StreamWriter(fullPath, append: true, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private void CloseWriter()
    {
        writer?.Dispose();
        writer = null;
    }

    private void DispatchEntry(string entry)
    {
        var handlers = EntryRecorded;
        if (handlers is null)
            return;

        foreach (Action<string> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(entry);
            }
            catch
            {
                // Diagnostic observers must never be able to interrupt serial communications.
            }
        }
    }
}
