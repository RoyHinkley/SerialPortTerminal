namespace SerialPortTerminal.Serial;

/// <summary>Describes the lifecycle state of the physical serial transport.</summary>
/// <remarks>
/// This is intentionally separate from connection intent and communications health. A requested
/// connection may remain requested while the transport is unavailable and being recovered.
/// </remarks>
public enum SerialConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting,
    Unavailable,
    Recovering,
    Disposed
}
