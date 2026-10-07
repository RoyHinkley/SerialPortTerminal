# SerialPortTerminal project notes

> Temporary live coordination document. Capture unresolved decisions and deferred ideas so they are not lost accidentally. Items may be removed deliberately when we decide not to pursue them. Delete this file once mature documentation and GitHub issues can carry the remaining work.

## Purpose

SerialPortTerminal is both an interactive serial diagnostic/investigation tool and a proving ground for lower-level serial communications machinery used by applications.

The reusable serial layer must preserve evidence an ordinary application might hide: exact TX/RX bytes, CRC bytes and failures, framing behavior, timing, connection/disconnection events, pin changes, buffer problems, counters, and related diagnostics. Diagnostic capability is product functionality, not merely developer tracing.

The immediate practical motivation is troubleshooting a Eurotherm controller. The previous SPT lacked binary byte logging and CRC-byte visibility present in newer AeonHacs code.

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

Adapt selected current AeonHacs serial machinery rather than depending on AeonHacs.Core. Do not import broad Hacs lifecycle/state/global infrastructure or the old catch-all `Utility` class.

Preserve field-proven receive/framing algorithms until understood and characterized by tests. Modernize their surroundings first. Functional changes to delicate protocol logic should be explicit rather than incidental cleanup.

## Communications model

The UI distinguishes the **conversation** from evidence of **how the conversation was transported**.

### Send / Received

- Use **Send** and **Received** rather than assuming every received message answers a command.
- Send is above Received; Enter sends; Up/Down recalls command history.
- Always show received traffic, including unsolicited/datalogging reports.
- Display choices never alter authoritative received data.
- Received display modes: raw text, escaped text, bytes; more may come later.
- CRC acceptance and CRC presentation are separate concerns. A diagnostic user may choose to receive/display a message whose CRC failed, and independently choose whether the Received presentation includes the normally stripped CRC bytes.
- Normally follow the latest received entry, but do not yank the view downward after the user deliberately scrolls up.

### Authoritative received data

Move away from `Action<string>` as the authoritative receive boundary. Retain original bytes unchanged with useful metadata. In particular, preserve CRC bytes and CRC-validation outcome even when the ordinary presentation strips the CRC bytes or rejects a failed message.

```text
physical serial reads/chunks
        |
SerialDevice framing
        |
received data/frame records
        |
logical Received messages / presentation grouping
```

Reserve `ReceivedMessage` for a logical message believed complete. Use `ReceivedData`/`ReceivedFrame` for fragments if appropriate.

For clearly framed traffic, every recognized logical message begins a new Received display entry even without CR/LF. UI separation is independent of payload whitespace; preserve CR/LF and blank lines actually present in the payload.

For unframed/streaming traffic, OS/serial read boundaries are **not** message boundaries. Multiple reads belong to one continuing display entry until a genuine higher-level boundary such as disconnect/reset/session boundary or future explicit policy.

## Transport diagnostics and logging

Transport Diagnostics explains how traffic transpired: raw TX/RX bytes, chunks, timing, CRC details/errors, connection and pin events, buffer conditions, recovery, etc.

The **disk log is the authoritative transport-diagnostic record**. The UI is a live view/tail of the same ordered stream, not a separately generated representation.

`DiagnosticLog` now uses an ordered producer/consumer: producers timestamp/enqueue quickly; a worker writes to disk and publishes UI entries in order. Shutdown drains pending entries. Logging should not materially perturb the timing being diagnosed.

### Baseline vs detailed diagnostics

Automatic session logging is independent of diagnostic detail. The log continues when detailed transport events are disabled. Baseline events such as session/connection state, errors, commands and received messages should remain recorded. The UI checkbox means **Include detailed transport events**, not “enable logging.” `LogEverything` should eventually be replaced or clarified so this distinction is explicit in the model rather than accidental policy.

### Automatic session logs

- Lazily create the log on the first connection attempt; merely opening/closing SPT need not create one.
- One log spans the application/investigation run, including disconnect/reconnect cycles.
- Default directory: `Logs` relative to the application directory; configurable relative or absolute path later.
- Default automatic prefix: `spt-`; configurable later.
- Lexicographically sortable filename, e.g. `spt-2026-10-06_192735.log`.
- Default retention: 25 automatic logs; configurable later.
- Retention applies only to automatic-name matches; never silently delete unrelated/user-renamed files or the current log.
- Show the active filename in Transport Diagnostics.
- Future: allow editing the filename to rename the actual backing file, with validation/collision handling. A renamed file naturally falls outside automatic retention; no separate Keep metadata is initially necessary.
- Possible conveniences later: Open Log / Open Log Folder / export. Automatic capture remains primary.

Current settings defaults:

```text
LogFolder       = "Logs"
LogFilePrefix   = "spt-"
LogFilesToKeep  = 25
```

Settings are stored portably beside the application rather than under AppData.

## Configuration direction

Keep these concerns distinct:

1. Application preferences — log folder/prefix/retention, UI behavior.
2. Serial connection — port, baud, parity, data bits, stop bits, handshake, RTS.
3. Protocol — CRC, framing/termination, pacing, receive-silence behavior, CRC-error acceptance policy.
4. Received display — raw/escaped/bytes, whether to show CRC bytes, and future filters.
5. Diagnostic detail/presentation — distinct from Received display and from whether the automatic log exists.

Named device profiles may eventually retain complete connection + protocol setups for frequently encountered instruments, but do not build profiles yet.

The permanent left configuration panel is provisional. A flyout/drawer is favored once configuration behavior matures so the main workspace remains focused on Communications and Transport Diagnostics.

Customized/technical UI terms need discoverable explanations, probably tooltips and/or a small information icon rather than expanding every label. In particular, do not assume users know that `MS`/`MSB` means **most significant**. Explain bit order and byte order explicitly. Apply the same pattern to other specialized terms as they appear.

## CRC and framing: critical legacy requirement

Field hardware must support the unfortunate form:

```text
<message><termchar><crc>
```

where the termination character participates in CRC calculation. Do not simplify this to `<message><crc>[<termchar>]`.

CRC may be calculated incrementally as bytes arrive, and the termination-byte value can occur in payload or CRC bytes. Term-character detection and CRC validation are therefore necessarily entangled: a candidate terminator may need subsequent CRC bytes before it can be accepted; a failed candidate must allow parsing to continue.

CRC-error acceptance is protocol/application policy. Ordinary AeonHacs applications should normally reject/ignore messages whose CRC fails, but SPT needs a diagnostic option to forward/display them for inspection. This is independent of whether the Received presentation includes the CRC bytes themselves.

Do not assume that “no term char” and “silence framed” are intrinsically the same protocol choice. They are related in the current implementation, but a useful future mode may use a term character for validation while also using receive silence as a framing/boundary condition. Preserve this as an open design question until the receive behavior is characterized.

Do not casually rewrite `ProcessRx`. Characterize with synthetic tests covering ordinary terminated messages, embedded termination bytes, valid `<message><termchar><crc>`, false candidates, CRC failure, multiple messages per chunk, arbitrary chunk splits, and silence boundaries.

The current adapted receive implementation has **not yet been proven correct for the legacy termchar-before-CRC case**.

## Receive-path preservation

Current `SerialDevice` retains the broad AeonHacs strategy: lightweight `DataReceived` signaling, receive-event coalescing until short silence, block reads into a ring buffer, separate parsing worker, and optional silence framing. These came from field use and should not be replaced merely because simpler code looks nicer.

Review ring-buffer full/empty ambiguity and exact capacity around a 4096-byte read/wrap.

## Serial signals / RTS

Future diagnostics should expose conventional signals:

- inputs/status: CTS, DSR, DCD/CD, RI;
- timestamp input changes in Transport Diagnostics;
- manual RTS/DTR outputs where handshake ownership permits;
- make handshake ownership explicit.

RTS modes conceptually include Disabled, Enabled, Toggle. Continuously asserted RTS is used by some field devices as a parasitic power source and must remain supported.

`RTS_CONTROL_TOGGLE` is separate driver-controlled transmit assertion useful for some RS-485 arrangements. It remains unimplemented in the modern `System.IO.Ports` path and is deferred, not expendable. Prefer a supported Win32 route if practical; do not restore reflection into private `SerialPort` internals.

## UI / implementation principles

Avalonia is the UI framework. Serial/application behavior must remain testable without the window. Preserve functional organization, not the old WinForms pixel geometry; use proper layout rather than magic offsets.

The current UI is provisional and functionality takes precedence over polish. Text views still use `TextBox.Text +=`, which becomes O(N^2); replace with an append-friendly/bounded presentation. The disk diagnostic log remains complete even if the UI retains only a bounded live tail.

Display/detail controls should apply live where sensible rather than silently taking effect only on the next connection.

## Longer-term direction

These are possibilities to preserve, not commitments:

- If the serial portion settles into a genuinely general subsystem, consider extracting a small independent library shared by SPT and AeonHacs/applications rather than allowing either product to own a divergent canonical copy.
- Watch for clean general improvements developed here that should be taken back upstream to AeonHacs, without constraining SPT around AeonHacs compatibility during modernization.
- The current Receive/Process/Transmit thread model is a migration baseline, not necessarily the final concurrency architecture. After behavioral characterization, reconsider async/await or another cleaner model where it improves correctness/readability without losing block acquisition or timing behavior.
- Characterization tests are the bridge to deeper cleanup of acquisition, buffering, pacing, recovery, CRC/framing, and concurrency. Historical implementation details need not survive forever once required behavior is captured.
- Make eventual handoff possible without oral history: sharp-edge comments, tests demonstrating historical protocol peculiarities, clear reusable-engine/application-policy boundaries, and permanent architecture documentation where warranted.
- Device profiles may eventually provide convenient named complete setups for frequently used instruments.

## Current status

Implemented:

- modern .NET/C# Avalonia application; legacy WinForms preserved under tag `legacy-winforms`;
- currentized `SerialDevice`, incremental CRC, serial settings and data formatting;
- `TerminalSession` application boundary;
- Communications and Transport Diagnostics workspaces;
- Send/Received separation, Enter-to-send, command history;
- asynchronous ordered `DiagnosticLog`;
- lazy automatic per-run disk logging with portable defaults and 25-file retention;
- active log filename display;
- initial CRC protocol controls in the UI, applied when establishing a serial session;
- diagnostic detail control explicitly separated in the UI from automatic logging.

Important limitations/deferred work:

- received data still crosses the main boundary as `string`; authoritative byte records pending, which currently prevents Received from independently showing normally stripped CRC bytes;
- legacy `<message><termchar><crc>` behavior not yet characterized/proven;
- current UI couples “no term char” to silence framing; whether those should be independently selectable remains open;
- UI diagnostic/Received views need scalable append/bounded-tail behavior;
- settings exist but do not yet have a settings UI; log filename rename remains deferred;
- protocol controls are only the initial CRC subset; pacing/silence and other practical controls remain;
- serial signal monitoring/control absent;
- `RTS_CONTROL_TOGGLE` absent;
- some controls need live propagation;
- specialized protocol labels need tooltips/information affordances.

## Near-term priority

1. Introduce authoritative byte-oriented received records carrying original bytes plus CRC validity/metadata, so CRC acceptance and CRC-byte display can be independent.
2. Render Communications from retained bytes, including a Received-side Show CRC bytes option and correct framed-vs-streaming grouping/follow-tail behavior.
3. Characterize/fix CRC/termination behavior with tests before substantial parser cleanup, including the possible independence of silence framing and term-character validation.
4. Complete other protocol controls needed for current instrument troubleshooting.
5. Replace O(N^2) text accumulation with scalable bounded UI tails.
6. Add concise tooltips/information affordances for specialized terms such as most-significant bit/byte ordering.
7. Add serial signal diagnostics/control and later `RTS_CONTROL_TOGGLE`.
8. Add settings UI/log rename when it provides practical value; do not let configuration polish block core diagnostic functionality.
9. Return to broader UI polish after the diagnostic path is solid.

## Development practice

Work directly on `main` with small coherent commits. Preserve correctness and historical protocol behavior over convenience. Avoid naive O(N^2) algorithms and keep GUI-specific behavior in the GUI.

Public/API comments should document semantics, rationale, invariants, and surprising constraints rather than restating obvious code—especially behavior a competent programmer might otherwise “simplify” incorrectly.
