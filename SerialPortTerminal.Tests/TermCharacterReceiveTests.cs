using System.Reflection;
using SerialPortTerminal.Serial;

namespace SerialPortTerminal.Tests;

public sealed class TermCharacterReceiveTests
{
    private static CrcOptions TerminatedCrc() => new()
    {
        Polynomial = 0xA001,
        InitialValue = 0xFFFF,
        ExpectedResidue = 0x0000,
        PostInvert = false,
        MsBitFirst = false,
        MsByteFirst = false,
        TermChar = 0x03,
        OmitTermChar = false
    };

    [Fact]
    public void Parser_accepts_ordinary_valid_message()
    {
        var options = TerminatedCrc();
        var payload = new byte[] { 0x10, 0x20, 0x30 };
        var wire = new Crc(options).Append(payload);

        using var harness = new ProcessRxHarness(options);
        harness.Feed(wire);

        var received = harness.WaitForMessages(1).Single();
        Assert.True(received.CrcValid);
        Assert.Equal(payload, received.PayloadBytes.ToArray());
    }

    [Fact]
    public void Illegal_term_character_in_payload_is_rejected_and_parser_recovers()
    {
        var options = TerminatedCrc();
        // Aeon framing reserves ETX for the terminator (except when it occurs in either CRC byte).
        // This deliberately invalid message verifies rejection and recovery. The number of CRC-error
        // observations during resynchronization is an implementation detail, not a protocol invariant.
        var illegalPayload = new byte[] { 0x10, options.TermChar, 0x20, 0x30 };
        var invalidWire = new Crc(options).Append(illegalPayload);
        var validPayload = new byte[] { 0x41, 0x42, 0x43 };
        var validWire = new Crc(options).Append(validPayload);

        using var harness = new ProcessRxHarness(options);
        harness.Feed(invalidWire.Concat(validWire).ToArray());

        var received = harness.WaitForMessages(1).Single();
        Assert.True(received.CrcValid);
        Assert.Equal(validPayload, received.PayloadBytes.ToArray());
        Assert.True(harness.CrcErrors > 0);
    }

    [Fact]
    public void Parser_handles_term_character_in_low_crc_byte()
    {
        var options = TerminatedCrc();
        var (payload, wire) = FindCodeword(options, bytes => bytes[^3] == options.TermChar);

        using var harness = new ProcessRxHarness(options);
        harness.Feed(wire);

        var received = harness.WaitForMessages(1).Single();
        Assert.True(received.CrcValid);
        Assert.Equal(payload, received.PayloadBytes.ToArray());
    }

    [Fact]
    public void Parser_handles_term_character_in_high_crc_byte()
    {
        var options = TerminatedCrc();
        var (payload, wire) = FindCodeword(options, bytes => bytes[^2] == options.TermChar);

        using var harness = new ProcessRxHarness(options);
        harness.Feed(wire);

        var received = harness.WaitForMessages(1).Single();
        Assert.True(received.CrcValid);
        Assert.Equal(payload, received.PayloadBytes.ToArray());
    }

    [Fact]
    public void Parser_preserves_state_across_every_fragment_boundary()
    {
        var options = TerminatedCrc();
        var (payload, wire) = FindCodeword(options, bytes => bytes[^3] == options.TermChar);

        for (var split = 1; split < wire.Length; split++)
        {
            using var harness = new ProcessRxHarness(options);
            harness.Feed(wire[..split]);
            harness.Feed(wire[split..]);

            var received = harness.WaitForMessages(1).Single();
            Assert.True(received.CrcValid);
            Assert.Equal(payload, received.PayloadBytes.ToArray());
        }
    }

    [Fact]
    public void Parser_delivers_legal_back_to_back_messages_from_one_chunk()
    {
        var options = TerminatedCrc();
        var firstPayload = new byte[] { 0x10, 0x20, 0x30 };
        var secondPayload = FindCodeword(options, bytes => bytes[^2] == options.TermChar).Payload;
        Assert.DoesNotContain(options.TermChar, firstPayload);
        Assert.DoesNotContain(options.TermChar, secondPayload);

        var firstWire = new Crc(options).Append(firstPayload);
        var secondWire = new Crc(options).Append(secondPayload);
        var combined = firstWire.Concat(secondWire).ToArray();

        using var harness = new ProcessRxHarness(options);
        harness.Feed(combined);

        var received = harness.WaitForMessages(2);
        Assert.Equal(firstPayload, received[0].PayloadBytes.ToArray());
        Assert.Equal(secondPayload, received[1].PayloadBytes.ToArray());
        Assert.All(received, message => Assert.True(message.CrcValid));
    }

    [Fact]
    public void Realistic_multiline_payload_survives_every_fragment_boundary()
    {
        var options = TerminatedCrc();
        // Representative of the documented Aeon controller reports: textual payload, embedded spaces
        // and CR/LF, followed by binary CRC and ETX. CR/LF are payload; only ETX frames the message.
        var payload = System.Text.Encoding.ASCII.GetBytes(" 23.4  24.1  25.0\r\nM  50.00 a  37.25\r\n 22.8  23.0\r\n");
        Assert.DoesNotContain(options.TermChar, payload);
        var wire = new Crc(options).Append(payload);

        for (var split = 1; split < wire.Length; split++)
        {
            using var harness = new ProcessRxHarness(options);
            harness.Feed(wire[..split]);
            harness.Feed(wire[split..]);

            var received = harness.WaitForMessages(1).Single();
            Assert.True(received.CrcValid);
            Assert.Equal(payload, received.PayloadBytes.ToArray());
        }
    }

    [Fact]
    public void Staged_protocol_change_finishes_current_message_with_old_settings_and_applies_to_next()
    {
        var oldOptions = TerminatedCrc();
        var newOptions = TerminatedCrc();
        newOptions.Polynomial = 0x1021;
        var oldPayload = new byte[] { 0x41, 0x42, 0x43, 0x44 };
        var newPayload = new byte[] { 0x51, 0x52, 0x53, 0x54 };
        var oldWire = new Crc(oldOptions).Append(oldPayload);
        var newWire = new Crc(newOptions).Append(newPayload);

        using var harness = new ProcessRxHarness(oldOptions);
        harness.Feed(oldWire[..2]);
        Assert.True(harness.StageProtocolSettings(SerialProtocolSettings.From(newOptions, suppressCrcErrors: false)));
        harness.Feed(oldWire[2..]);

        var first = harness.WaitForMessages(1).Single();
        // A valid old-codeword response proves the in-progress message completed under the old snapshot.
        // Once that boundary is crossed, the staged snapshot should already be active for the next message.
        Assert.True(first.CrcValid);
        Assert.Equal(oldPayload, first.PayloadBytes.ToArray());
        Assert.Equal(newOptions.Polynomial, harness.ProtocolSettings.Polynomial);

        harness.Feed(newWire);
        var received = harness.WaitForMessages(2);
        Assert.True(received[1].CrcValid);
        Assert.Equal(newPayload, received[1].PayloadBytes.ToArray());
        Assert.Equal(newOptions.Polynomial, harness.ProtocolSettings.Polynomial);
    }

    private static (byte[] Payload, byte[] Wire) FindCodeword(CrcOptions options, Func<byte[], bool> predicate)
    {
        for (var value = 0; value <= ushort.MaxValue; value++)
        {
            var payload = new byte[] { (byte)value, (byte)(value >> 8) };
            if (payload.Contains(options.TermChar)) continue;
            var wire = new Crc(options).Append(payload);
            if (predicate(wire)) return (payload, wire);
        }

        throw new InvalidOperationException("Unable to synthesize requested CRC edge case.");
    }

    /// <summary>
    /// Characterization harness for the inherited ProcessRx worker. Reflection is intentional here:
    /// it lets tests drive the real parser without adding a test-only API or refactoring delicate production code.
    /// </summary>
    private sealed class ProcessRxHarness : IDisposable
    {
        private const int WaitMilliseconds = 2000;
        private readonly SerialDevice device;
        private readonly FieldInfo activeField = Field("active");
        private readonly FieldInfo rxField = Field("rx");
        private readonly FieldInfo rxbWriteField = Field("rxbWrite");
        private readonly FieldInfo rxCrcField = Field("rxCrc");
        private readonly FieldInfo protocolSettingsField = Field("protocolSettings");
        private readonly FieldInfo processSignalField = Field("processSignal");
        private readonly AutoResetEvent processSignal;
        private readonly Thread worker;
        private readonly List<ReceivedData> messages = [];
        private readonly object sync = new();

        public ProcessRxHarness(CrcOptions options)
        {
            device = new SerialDevice("TEST") { CrcConfig = options };
            device.DataReceived += OnDataReceived;
            protocolSettingsField.SetValue(device, SerialProtocolSettings.From(options, suppressCrcErrors: false));
            rxCrcField.SetValue(device, new Crc(options));
            activeField.SetValue(device, true);
            processSignal = (AutoResetEvent)processSignalField.GetValue(device)!;
            var processRx = typeof(SerialDevice).GetMethod("ProcessRx", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(typeof(SerialDevice).FullName, "ProcessRx");
            worker = new Thread(() => processRx.Invoke(device, null)) { IsBackground = true };
            worker.Start();
            WaitUntilWorkerIsWaiting();
        }

        public uint CrcErrors => device.CRCErrors;
        public SerialProtocolSettings ProtocolSettings => device.ProtocolSettings;
        public bool StageProtocolSettings(SerialProtocolSettings settings) => device.StageProtocolSettings(settings);

        public void Feed(ReadOnlySpan<byte> bytes)
        {
            var rx = (byte[])rxField.GetValue(device)!;
            var write = (int)rxbWriteField.GetValue(device)!;
            foreach (var value in bytes)
            {
                rx[write] = value;
                write = (write + 1) % rx.Length;
            }
            rxbWriteField.SetValue(device, write);
            processSignal.Set();
        }

        public IReadOnlyList<ReceivedData> WaitForMessages(int count)
        {
            var deadline = Environment.TickCount64 + WaitMilliseconds;
            lock (sync)
            {
                while (messages.Count < count)
                {
                    var remaining = deadline - Environment.TickCount64;
                    if (remaining <= 0) throw new TimeoutException($"Expected {count} messages; received {messages.Count}.");
                    Monitor.Wait(sync, (int)Math.Min(remaining, int.MaxValue));
                }
                return messages.ToArray();
            }
        }

        private void OnDataReceived(ReceivedData data)
        {
            lock (sync)
            {
                messages.Add(data);
                Monitor.PulseAll(sync);
            }
        }

        private void WaitUntilWorkerIsWaiting()
        {
            var deadline = Environment.TickCount64 + WaitMilliseconds;
            while (Environment.TickCount64 < deadline)
            {
                if ((worker.ThreadState & ThreadState.WaitSleepJoin) != 0) return;
                Thread.Yield();
            }
            throw new TimeoutException("ProcessRx worker did not reach its initial wait state.");
        }

        public void Dispose()
        {
            activeField.SetValue(device, false);
            processSignal.Set();
            worker.Join(WaitMilliseconds);
            device.DataReceived -= OnDataReceived;
            device.Dispose();
        }

        private static FieldInfo Field(string name) => typeof(SerialDevice).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(SerialDevice).FullName, name);
    }
}