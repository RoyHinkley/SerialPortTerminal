using SerialPortTerminal.Serial;

namespace SerialPortTerminal.Tests;

public sealed class SilenceFramedReceiveTests
{
    private static CrcOptions FieldCrc() => new()
    {
        Polynomial = 0xA001,
        InitialValue = 0xFFFF,
        ExpectedResidue = 0x0000,
        PostInvert = false,
        MsBitFirst = false,
        MsByteFirst = false,
        OmitTermChar = true
    };

    [Fact]
    public void Valid_crc_response_preserves_wire_payload_and_crc_evidence()
    {
        using var device = new SerialDevice("TEST") { CrcConfig = FieldCrc() };
        var payload = new byte[] { 0x02, 0x03, 0x10, 0x03, 0x20 };
        var wire = new Crc(FieldCrc()).Append(payload);

        var received = device.DecodeSilenceFramedMessage(wire);

        Assert.True(received.CrcValid);
        Assert.Equal(wire, received.WireBytes.ToArray());
        Assert.Equal(payload, received.PayloadBytes.ToArray());
        Assert.Equal(wire[^2..], received.CrcBytes.ToArray());
        Assert.Equal((ushort)0x0000, received.CalculatedResidue);
        Assert.Equal((ushort)0x0000, received.ExpectedResidue);
    }

    [Fact]
    public void Corrupt_crc_is_reported_without_losing_received_bytes()
    {
        using var device = new SerialDevice("TEST") { CrcConfig = FieldCrc() };
        var payload = new byte[] { 0x02, 0x03, 0x10, 0x20 };
        var wire = new Crc(FieldCrc()).Append(payload);
        wire[^1] ^= 0x01;

        var received = device.DecodeSilenceFramedMessage(wire);

        Assert.False(received.CrcValid);
        Assert.Equal(wire, received.WireBytes.ToArray());
        Assert.Equal(payload, received.PayloadBytes.ToArray());
        Assert.NotEqual(received.ExpectedResidue, received.CalculatedResidue);
    }

    [Fact]
    public void No_crc_configuration_delivers_all_bytes_as_payload()
    {
        using var device = new SerialDevice("TEST");
        var wire = new byte[] { 0x02, 0x03, 0x00, 0xFF, 0x03 };

        var received = device.DecodeSilenceFramedMessage(wire);

        Assert.Null(received.CrcValid);
        Assert.Equal(wire, received.WireBytes.ToArray());
        Assert.Equal(wire, received.PayloadBytes.ToArray());
        Assert.Empty(received.CrcBytes.ToArray());
    }

    [Fact]
    public void Virtual_instrument_response_with_embedded_03_decodes_as_one_silence_framed_message()
    {
        var crcOptions = FieldCrc();
        var command = new byte[] { 0x02, 0x03, 0x00, 0x01, 0x00, 0x02 };
        var payload = new byte[] { 0x02, 0x03, 0x11, 0x03, 0x22, 0x33 };
        var response = new Crc(crcOptions).Append(payload);
        var instrument = new VirtualInstrument().RespondTo(command, response);
        using var device = new SerialDevice("TEST") { CrcConfig = crcOptions };

        var received = device.DecodeSilenceFramedMessage(instrument.Respond(command));

        Assert.True(received.CrcValid);
        Assert.Equal(payload, received.PayloadBytes.ToArray());
    }
}
