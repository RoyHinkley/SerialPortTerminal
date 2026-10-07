# SerialPortTerminal project notes

> Temporary live project document. Keep this current while the modernization is still fluid. Once the design and implementation have matured enough for normal GitHub issues/documentation to carry the remaining work, delete this file.

## Purpose

SerialPortTerminal is both an interactive serial diagnostic/investigation tool and a proving ground for the lower-level serial communications machinery used by applications.

The reusable serial layer must therefore preserve evidence that an ordinary application might hide: exact transmitted and received bytes, CRC bytes and failures, framing behavior, timing, connection/disconnection events, pin changes, buffer problems, and related counters. Diagnostic capability is a product feature, not merely development tracing.

The immediate practical motivation is troubleshooting a Eurotherm controller. The previous SerialPortTerminal lacked binary byte logging and CRC-byte visibility that exist in newer AeonHacs code.

## Architecture direction

Keep the serial engine independent of Avalonia and application policy.

```text
SerialDevice
    transport / receive acquisition / framing / CRC
    pacing / recovery / low-level diagnostics
        |
        +--> TerminalSession --> SerialPortTerminal UI
        |
        +--> reusable by eventual applications/controllers
```

Selected current AeonHacs serial machinery is being adapted rather than importing AeonHacs.Core as a dependency. SerialPortTerminal should remain self-contained. Do not import broad Hacs lifecycle/state/global infrastructure or the old catch-all `Utility` class.

Migration principle: preserve field-proven receive/framing algorithms until they are understood and characterized by tests. Modernize their surroundings first. Functional changes to delicate protocol logic should be explicit rather than incidental cleanup.

## Communications model

The UI should distinguish the **conversation** from the detailed evidence of **how the conversation was transported**.

### Sent / Received

The Communications workspace is the operator's record of the conversation.

- Label the two directions **Send** and **Received**, rather than assuming every received message is a response to a command.
- Send belongs above Received.
- Enter in the Send control sends the current command.
- Maintain command history and allow previous commands to be recalled.
- Always show received messages, including unsolicited traffic and datalogging reports.
- Display choices affect presentation only; they must never alter the authoritative received data.
- Received display modes currently envisioned: raw text, escaped text, and bytes. More presentation/filter modes may be added later.
- The latest received entry should normally remain visible by automatic scrolling. If the user deliberately scrolls upward to inspect history, new traffic should not forcibly pull the view back to the bottom.

### Authoritative received data

Move away from `Action<string>` as the authoritative receive boundary. Received data should retain the original bytes unchanged, accompanied by useful metadata.

A future receive model should distinguish:

```text
physical serial reads/chunks
        |
SerialDevice framing
        |
received data/frame records
        |
logical Received messages / presentation grouping
```

A `ReceivedMessage` should mean a logical message we actually believe is complete. If lower-level records may represent only fragments, use a name such as `ReceivedData` or `ReceivedFrame` rather than calling every OS read a message.

For clearly framed traffic, each recognized message begins a new Received display entry even when its payload contains no CR/LF. UI entry separation is independent of payload whitespace. If a message itself contains CR/LF or blank lines, preserve those bytes/whitespace; do not consume them merely because the UI also separates entries.

For unframed/streaming traffic where distinct messages cannot be discriminated, serial/OS read boundaries must **not** become display-message boundaries. Multiple receive chunks should be presented as one continuing/wrapping Received entry until a genuine higher-level boundary exists (for example disconnect/reset/session boundary or future explicit policy).

This distinction is important for both command/response devices and continuous/datalogging devices.

## Transport diagnostics

Transport Diagnostics is separate from Communications. It explains how the traffic transpired: raw TX/RX bytes, read chunks, timing, CRC details/errors, connection events, pin events, buffer conditions, recovery, and similar information.

The UI has a Transport Diagnostics tab so this information can be inspected without opening an external editor. Historically these diagnostics existed only in log files.

The **disk log is the authoritative transport-diagnostic record**. The UI view should be a live view/tail of that same ordered diagnostic stream, not an independently generated second version.

Logging must not materially perturb the serial timing being diagnosed. The current `DiagnosticLog` performs synchronous file I/O/event delivery and should be changed to an ordered producer/consumer design: serial paths enqueue timestamped records quickly; a background consumer writes/publishes them in order. Disposal must drain pending records. File failures should be isolated and surfaced without recursive logging.

### Automatic session logs

Logging should require essentially no operator forethought.

- Lazily create a diagnostic log when something interesting first happens, initially the first connection attempt. Merely opening and closing the application need not create a log.
- Use one log for the application/investigation session, spanning disconnect/reconnect cycles. Those transitions are useful evidence.
- Default log directory: `Logs` relative to the application directory, supporting the portable-application model.
- Allow the user to configure another relative or absolute log directory.
- Default automatic filename prefix: `spt-`.
- Default timestamp format is lexicographically sortable, e.g. `spt-2026-10-06_192735.log`.
- Make the filename prefix configurable.
- Default retention: 25 automatically named session logs; make this configurable.
- Retention should apply only to files matching the configured automatic naming convention. Do not silently delete unrelated/user-renamed files.
- Never delete the current log.
- Show the current log filename in the Transport Diagnostics UI.
- Allow the user to rename the current log. Renaming means renaming the actual backing file, with collision/filename validation.
- A renamed log naturally becomes a retained/important log because it no longer matches the automatic retention pattern; no separate Keep metadata is required initially.
- Later conveniences may include Open Log / Open Log Folder / export, but automatic capture is primary.

Initial persistent logging settings:

```text
LogFolder       = "Logs"
LogFilePrefix   = "spt-"
LogFilesToKeep  = 25
```

## Configuration direction

Configuration is becoming a real application concept and should not remain hard-coded in the window.

Separate at least these concerns:

1. **Application preferences** — log folder, prefix, retention, UI behavior.
2. **Serial connection settings** — port, baud, parity, data bits, stop bits, handshake, RTS behavior.
3. **Protocol settings** — CRC, framing/termination, pacing, receive-silence behavior, CRC-error policy.
4. **Received display settings** — raw/escaped/byte presentation and future filters.
5. **Diagnostic logging/presentation settings** — distinct from Received display.

Named device profiles may eventually be useful for connection/protocol settings, but do not build that system yet.

The current permanent left configuration panel is provisional. A left flyout/drawer is favored over consuming permanent workspace once configuration behavior is mature. The main workspace should remain focused on Communications and Transport Diagnostics.

## CRC and framing: critical legacy requirement

There is field hardware/protocol behavior that must support the unfortunate form:

```text
<message><termchar><crc>
```

where the termination character participates in the CRC calculation. This cannot be simplified to the more conventional `<message><crc>[<termchar>]` assumption.

CRC may be calculated incrementally as bytes arrive, and the termination byte value can also occur in payload or CRC bytes. Consequently, term-character detection and CRC validation are necessarily entangled: a candidate term character may have to be followed by CRC bytes and validated before it can be accepted as the real message boundary; a failed candidate must allow parsing to continue.

Do not casually rewrite `ProcessRx`. Characterize the required behavior first with synthetic tests, including:

- ordinary terminated messages;
- embedded termination-byte values;
- valid `<message><termchar><crc>`;
- false termination candidates;
- CRC failure;
- multiple messages in one received chunk;
- arbitrary chunk splits;
- silence-based boundaries.

The current adapted receive implementation has **not yet been proven correct for this legacy termchar-before-CRC case**.

## Receive-path preservation notes

The current SerialDevice retains the broad AeonHacs receive strategy:

- `DataReceived` notification remains lightweight and signals receive work.
- receive notifications are coalesced until a short silence;
- data is read in blocks into a ring buffer;
- parsing occurs on a separate worker thread;
- silence may itself be the logical boundary for un-terminated protocols.

These behaviors came from field use and should not be replaced with aesthetically simpler code without measurement/tests.

Known area to review: ring-buffer full/empty ambiguity and exact capacity behavior around a 4096-byte read/wrap.

## Serial signals / RTS

Future diagnostics should expose conventional serial signals because they are useful during hardware troubleshooting:

- inputs/status: CTS, DSR, DCD/CD, RI;
- timestamp input changes in Transport Diagnostics;
- outputs: manual RTS and DTR where handshake ownership permits;
- make handshake ownership explicit in the UI.

RTS modes conceptually include Disabled, Enabled, and Toggle.

Ordinary continuously asserted/deasserted RTS is also used by some field devices as a parasitic power source and must remain supported.

`RTS_CONTROL_TOGGLE` is a separate driver-controlled transmit assertion behavior useful for some RS-485 arrangements. It is currently unimplemented in the modern `System.IO.Ports` version and throws `NotSupportedException`. It is deferred, not expendable. Prefer a supported Win32 route if practical; do not restore fragile reflection into private `SerialPort` internals.

## UI / implementation principles

Avalonia is the UI framework. Keep serial/application behavior testable without the window.

Functional organization matters more than reproducing the old WinForms pixel layout. Use real layout containers, consistent alignment/spacing, and avoid magic pixel fixes.

Current broad UI direction:

```text
[ configuration / future flyout ] [ Communications | Transport Diagnostics ]

Communications:
    Send
    Received

Transport Diagnostics:
    active log filename
    live diagnostic view/tail
```

The current UI is deliberately provisional; functionality takes precedence over polish.

The current text views append by repeatedly replacing `TextBox.Text` (`Text +=`), which becomes O(N^2) as a session grows. Replace this with an append-friendly/bounded presentation mechanism. The diagnostic disk log remains complete even if the UI retains only a bounded live tail.

Display/logging controls should eventually apply live where sensible rather than silently taking effect only on the next connection.

## Current implementation status

The legacy WinForms implementation is preserved in Git history/tag `legacy-winforms`; obsolete source files have been removed from the modern tree.

The repository is a flat single-project layout targeting current .NET/C# with Avalonia.

Implemented foundations include:

- `Serial/Crc.cs` — incremental CRC support and options;
- `Serial/SerialPortSettings.cs` — pure serial-port settings value;
- `Serial/SerialDataFormatter.cs` — text/escaped/byte formatting;
- `Serial/SerialDevice.cs` — currentized serial engine with transmit/receive workers, pacing, CRC hooks, diagnostics, and counters;
- `Diagnostics/DiagnosticLog.cs` — initial logging abstraction (currently synchronous; redesign pending);
- `Application/TerminalSession.cs` — application-level serial-session boundary;
- Avalonia UI with connection controls, Communications and Transport Diagnostics workspaces, Send/Received separation, Enter-to-send, and initial command history.

Important current limitations:

- received data still crosses the main boundary as `string`; authoritative byte-oriented receive records are pending;
- legacy `<message><termchar><crc>` behavior is not yet characterized/proven;
- diagnostic logging is not yet automatically persisted and is currently synchronous;
- diagnostic/Received text views need scalable append/bounded-tail behavior;
- automatic log naming/retention/settings are not yet implemented;
- CRC/protocol controls are not yet exposed in the UI;
- serial signal monitoring/control is not yet exposed;
- `RTS_CONTROL_TOGGLE` remains unimplemented;
- some display/logging controls need live propagation.

## Near-term priority

1. Introduce application settings, initially including log folder/prefix/retention.
2. Redesign `DiagnosticLog` as ordered asynchronous producer/consumer.
3. Lazily create the automatic session log on first connection attempt; make it authoritative and apply retention.
4. Make Transport Diagnostics a scalable live tail of that log and support backing-file rename.
5. Introduce authoritative byte-oriented received-data/message records without casually changing framing semantics.
6. Make Communications presentation render from those retained bytes, including correct framed-vs-streaming entry grouping and follow-tail behavior.
7. Expose the protocol controls needed for practical Eurotherm troubleshooting.
8. Characterize/fix CRC/termination behavior with tests before substantial receive-parser cleanup.
9. Add serial signal diagnostics/control and later `RTS_CONTROL_TOGGLE`.
10. Return to UI polish after the functional diagnostic path is solid.

## Development practice

Work directly on `main` with small coherent commits. Preserve correctness and historical protocol behavior over convenience. Avoid naive O(N^2) algorithms and keep GUI-specific behavior in the GUI rather than pushing it into the serial engine.

Public/API comments should document semantics, rationale, invariants, and surprising constraints rather than restating obvious code. In particular, document behavior that a competent programmer might otherwise be tempted to "simplify" incorrectly.

This file is intentionally a temporary coordination document, not permanent product documentation. Keep it synchronized with important decisions and delete it once mature documentation plus GitHub issues are sufficient.