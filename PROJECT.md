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

## Configuration ownership and persistence

`TerminalConfiguration` is the authoritative live configuration. The UI binds to it; serial sessions, diagnostic logging, and persistence consume it directly rather than interrogating controls.

The configuration is deliberately serializable as a plain JSON object. The same representation should support:

- automatic restoration of the last session configuration;
- named saved device profiles later;
- Save As / Load / Delete / Revert profile operations later;
- easy inspection, comparison, exchange, and bug-report attachment without a proprietary format.

The last configuration is currently restored at startup and saved at clean shutdown as `SerialPortTerminal.configuration.json` beside the application. A missing previously selected COM port does not invalidate or overwrite the rest of the restored configuration.

Configuration changes are semantic events carrying old/new values. Once a diagnostic session log exists, changes are recorded there. Log creation records an initial configuration snapshot, and every connection attempt records the effective connection/protocol configuration so a diagnostic log remains interpretable even when settings were established before logging began.

Keep these concerns conceptually distinct even though the current configuration object is their common interface:

1. Application preferences — log folder/prefix/retention, UI behavior.
2. Serial connection — port, baud, parity, data bits, stop bits, handshake, RTS.
3. Protocol — CRC, framing/termination, pacing, receive-silence behavior, CRC-error acceptance policy.
4. Received display — raw/escaped/bytes, whether to show CRC bytes, and future filters.
5. Diagnostic detail/presentation — distinct from Received display and from whether the automatic log exists.

### Deferred configuration conveniences

Prioritize these only after replacement-critical communications behavior is sound:

- named device profiles containing complete useful setups;
- dirty-state indication for a loaded profile and one-click revert;
- import/export naturally using the same JSON representation;
- persistent command history, preferably profile-specific once profiles exist;
- named/preset commands per profile;
- timestamped user annotations/bookmarks in the diagnostic stream (for physical actions such as swapping leads, changing an instrument address, or marking a reproduced failure).

The permanent left configuration panel is provisional. A flyout/drawer is favored once configuration behavior matures so the main workspace remains focused on Communications and Transport Diagnostics.

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

`ReceivedData` carries original recognized-frame bytes, presentation payload bytes, and CRC validity instead of reducing the receive boundary to `string`. Continue enriching this record as framing behavior is characterized; in particular, verify exact terminator inclusion and invalid-CRC payload/CRC separation.

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

For unframed/streaming traffic, OS/serial read boundaries are **not** message boundaries. Multiple reads belong to one continuing display entry until a genuine higher-level boundary such as disconnect/reset/session boundary or future explicit policy. Current silence-framed dispatch still needs review against this requirement.

## Transport diagnostics and logging

Transport Diagnostics explains how traffic transpired: raw TX/RX bytes, chunks, timing, CRC details/errors, connection and pin events, buffer conditions, recovery, configuration changes, etc.

The **disk log is the authoritative transport-diagnostic record**. The UI is a live view/tail of the same ordered stream, not a separately generated representation.

`DiagnosticLog` uses an ordered producer/consumer: producers timestamp/enqueue quickly; a worker writes to disk and publishes UI entries in order. Shutdown drains pending entries. Logging should not materially perturb the timing being diagnosed.

Normal successful CRC validation and absence of CRC are not noteworthy conditions in response log lines. Report CRC status there only when an error is detected. Attempts to send without a connected device are timestamped diagnostic events, not unstructured UI text.

### Baseline vs detailed diagnostics

Automatic session logging is independent of diagnostic detail. The log continues when detailed transport events are disabled. Baseline events such as session/connection state, errors, commands and received messages should remain recorded. The UI checkbox means **Include detailed transport events**, not “enable logging.” `LogEverything` should eventually be replaced or clarified so this distinction is explicit in the model rather than accidental policy.

### Automatic session logs

- Lazily create the log on the first connection attempt or another event important enough to persist, such as a rejected send.
- One log spans the application/investigation run, including disconnect/reconnect cycles.
- Default directory: `Logs` relative to the application directory; configurable relative or absolute path later.
- Default automatic prefix: `spt-`; configurable later.
- Lexicographically sortable filename, e.g. `spt-2026-10-06_192735.log`.
- Default retention: 25 automatic logs; configurable later.
- Retention applies only to automatic-name matches; never silently delete unrelated/user-renamed files or the current log.
- Show the active filename in Transport Diagnostics.
- Future: allow editing the filename to rename the actual backing file, with validation/collision handling. A renamed file naturally falls outside automatic retention; no separate Keep metadata is initially necessary.
- Possible conveniences later: Open Log / Open Log Folder / export. Automatic capture remains primary.

Current application settings defaults:

```text
LogFolder       = "Logs"
LogFilePrefix   = "spt-"
LogFilesToKeep  = 25
```

Application settings and terminal configuration are stored portably beside the application rather than under AppData.

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

The current adapted receive implementation has **not yet been proven correct for the legacy termchar-before-CRC case**. This is replacement-critical.

## Receive-path preservation / AeonHacs incorporation

Current `SerialDevice` retains the broad current AeonHacs strategy: lightweight `DataReceived` signaling, receive-event coalescing until short silence, block reads into a ring buffer, separate parsing worker, incremental CRC, transmit pacing machinery, counters/timing, and optional silence framing. These came from field use and should not be replaced merely because simpler code looks nicer.

Useful AeonHacs improvements already incorporated/adapted include binary-safe byte formatting/logging, current receive/coalescing structure, incremental CRC machinery, message/byte pacing in the engine, CRC-error forwarding policy, and transport counters/timing. SPT deliberately replaces Hacs globals/Notify/LogFile/Utility dependencies with local boundaries.

Still to assess or expose from current Hacs where useful:

- complete pacing/silence controls in the SPT configuration/UI;
- serial-port arrival/removal monitoring and discovery behavior;
- signal/pin diagnostics;
- `RTS_CONTROL_TOGGLE` implementation;
- any recovery/reset behavior not yet represented by the adapted engine.

Review ring-buffer full/empty ambiguity and exact capacity around a 4096-byte read/wrap.

## Serial signals / RTS

Future diagnostics should expose conventional signals:

- inputs/status: CTS, DSR, DCD/CD, RI;
- timestamp input changes in Transport Diagnostics;
- manual RTS/DTR outputs where handshake ownership permits;
- make handshake ownership explicit.

RTS modes conceptually include Disabled, Enabled, Toggle. Continuously asserted RTS is used by some field devices as a parasitic power source and must remain supported.

`RTS_CONTROL_TOGGLE` is separate driver-controlled transmit assertion useful for some RS-485 arrangements. It remains unimplemented in the modern `System.IO.Ports` path and is deferred, not expendable. Prefer a supported Win32 route if practical; do not restore reflection into private `SerialPort` internals.

## Legacy functional parity / replacement readiness

The modernization is close enough to begin treating **replacement readiness** as the critical milestone. The old WinForms implementation remains available under tag `legacy-winforms`.

### Already at parity or better

- ordinary serial-port configuration and connect/disconnect;
- interactive command sending and Up/Down command history;
- received traffic display;
- CRC enable/configuration and CRC-error forwarding option;
- byte-oriented Received display and optional CRC-byte visibility, which improves on the stale legacy SPT;
- substantially richer ordered transport diagnostics and automatic persistent session logs;
- binary TX/RX byte logging from newer Hacs behavior;
- modern separation of reusable serial engine, terminal/session behavior, configuration, and Avalonia UI;
- portable restoration of the previous terminal configuration.

### Legacy behavior still missing or needing an explicit decision

- byte pacing and message pacing are present in `SerialDevice` but not yet exposed in the modern configuration/UI; legacy SPT exposed both;
- legacy Reset cleared counters/display and recreated the serial device; modern equivalent behavior has not yet been provided/decided;
- legacy showed command/response/bytes-read counters; modern diagnostics contain much of the evidence but there is no equivalent compact status display;
- legacy displayed calculated TX/RX CRC values separately; modern diagnostics/CRC-byte display may make those redundant, but decide consciously rather than losing them accidentally;
- legacy had a separate user-controlled normalized data-recording file (`Start Logging` / `Stop Logging`) distinct from transport diagnostics. Decide whether this workflow is still useful; do not confuse it with the richer automatic diagnostic log;
- legacy automatically reconfigured/reconnected when communication settings differed at send time; modern SPT deliberately locks connection/protocol controls while connected and requires explicit connection lifecycle. This is likely clearer, but is a conscious behavior change;
- legacy Escape cleared the command editor/history position; modern command-entry ergonomics do not yet duplicate that exact behavior.

### Replacement-critical technical gaps

Before declaring the legacy version superseded for general field use:

1. Characterize and prove/fix CRC/framing, especially `<message><termchar><crc>`, embedded candidate terminators, arbitrary chunking, and CRC failure.
2. Expose pacing/silence settings needed by instruments that depend on them.
3. Verify Received grouping for unframed/streaming traffic.
4. Exercise reconnect/reset/error paths and remove misleading lifecycle diagnostics such as redundant disconnect messages from replacing an already disconnected device.
5. Decide the few legacy functions above rather than accidentally dropping them.

Signals, `RTS_CONTROL_TOGGLE`, scalable UI tails, named profiles, and UI polish are important but need not all block initial replacement use unless a target instrument specifically requires them.

## UI / implementation principles

Avalonia is the UI framework. Serial/application behavior must remain testable without the window. Preserve functional organization, not the old WinForms pixel geometry; use proper layout rather than magic offsets.

The current UI is provisional and functionality takes precedence over polish. Text views still use `TextBox.Text +=`, which becomes O(N^2); replace with an append-friendly/bounded presentation. The disk diagnostic log remains complete even if the UI retains only a bounded live tail.

Display/detail controls should apply live where sensible rather than silently taking effect only on the next connection.

Customized/technical UI terms need discoverable explanations, probably tooltips and/or a small information icon rather than expanding every label. In particular, do not assume users know that `MS`/`MSB` means **most significant**. Explain bit order and byte order explicitly. Apply the same pattern to other specialized terms as they appear.

## Deployment / installation

A Windows x64 self-contained publish profile is provided as the first deployment mechanism. It favors a portable installation: no separate .NET runtime prerequisite, and configuration/logs remain alongside the application as intended.

Publish with the `Windows-x64` profile (or equivalent `dotnet publish` command). It currently requests a self-contained single-file Release build with ReadyToRun enabled. Validate the produced package on a clean Windows machine before calling it an installer/release.

For the immediate replacement milestone, a self-contained portable package is preferable to spending time on MSI/MSIX machinery. A conventional installer, shortcuts, file associations, signing, update mechanism, and architecture variants can be considered after the application is functionally trustworthy. If eventually installed under a protected location such as Program Files, the current beside-the-executable writable settings/log policy will need reconsideration or an explicitly user-writable data directory.

## Longer-term direction

These are possibilities to preserve, not commitments:

- If the serial portion settles into a genuinely general subsystem, consider extracting a small independent library shared by SPT and AeonHacs/applications rather than allowing either product to own a divergent canonical copy.
- Watch for clean general improvements developed here that should be taken back upstream to AeonHacs, without constraining SPT around AeonHacs compatibility during modernization.
- The current Receive/Process/Transmit thread model is a migration baseline, not necessarily the final concurrency architecture. After behavioral characterization, reconsider async/await or another cleaner model where it improves correctness/readability without losing block acquisition or timing behavior.
- Characterization tests are the bridge to deeper cleanup of acquisition, buffering, pacing, recovery, CRC/framing, and concurrency. Historical implementation details need not survive forever once required behavior is captured.
- Make eventual handoff possible without oral history: sharp-edge comments, tests demonstrating historical protocol peculiarities, clear reusable-engine/application-policy boundaries, and permanent architecture documentation where warranted.

## Current status

Implemented:

- modern .NET/C# Avalonia application; legacy WinForms preserved under tag `legacy-winforms`;
- currentized `SerialDevice`, incremental CRC, serial settings and data formatting;
- `TerminalSession` application boundary;
- authoritative byte-oriented `ReceivedData` with CRC status/presentation bytes;
- Communications and Transport Diagnostics workspaces;
- Send/Received separation, Enter-to-send, command history;
- asynchronous ordered `DiagnosticLog`;
- lazy automatic per-run disk logging with portable defaults and 25-file retention;
- active log filename display and diagnostic word-wrap control;
- CRC protocol controls, CRC-error delivery, Received CRC-byte display;
- diagnostic detail control explicitly separated from automatic logging;
- authoritative `TerminalConfiguration` bound by the UI, serializable to JSON, restored from the last session, and logged when changed;
- initial/effective configuration snapshots in diagnostic logs;
- self-contained Windows x64 publish profile.

Important limitations/deferred work are captured in the replacement-readiness and configuration sections above rather than maintained as a second competing list.

## Near-term priority

Priority is now driven first by **safe legacy replacement / Hacs parity**, then usability, then convenience:

1. Build CRC/framing characterization tests and prove/fix the legacy termchar-before-CRC behavior and chunk-boundary cases.
2. Expose byte/message pacing and receive-silence controls through `TerminalConfiguration` and the UI; verify the adapted Hacs behavior.
3. Correct unframed/streaming Received grouping and exercise reconnect/reset/error lifecycle behavior.
4. Review the remaining legacy behaviors (Reset/counters, separate data recording, explicit CRC-value display, command-entry details) and either implement or deliberately retire each.
5. Produce and smoke-test the self-contained Windows package on a clean machine; begin using it for real diagnostic work where the target protocol is covered.
6. Replace O(N^2) UI text accumulation with scalable bounded live tails and correct follow-tail behavior.
7. Add concise tooltips/information affordances for specialized terms such as most-significant bit/byte ordering.
8. Add named configuration profiles with Save/Load/dirty/revert behavior.
9. Add serial signal diagnostics/control and later `RTS_CONTROL_TOGGLE` as target hardware requires.
10. Add convenience features such as persistent/profile-specific command history, preset commands, annotations, log-folder/open conveniences, and broader UI polish after the diagnostic path is solid.

## Development practice

Work directly on `main` with small coherent commits. Preserve correctness and historical protocol behavior over convenience. Avoid naive O(N^2) algorithms and keep GUI-specific behavior in the GUI.

Public/API comments should document semantics, rationale, invariants, and surprising constraints rather than restating obvious code—especially behavior a competent programmer might otherwise “simplify” incorrectly.
