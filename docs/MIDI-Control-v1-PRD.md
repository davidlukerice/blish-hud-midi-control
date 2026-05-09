# PRD: Blish HUD MIDI Control v1

**Status:** ready-for-agent

---

## Problem Statement

Guild Wars 2 players who own a MIDI keyboard or controller currently have no native way to map MIDI note messages to in-game instrument keypresses. This forces them to run a standalone Electron application as a separate process, which adds overhead, window management friction, and does not integrate with the game's overlay ecosystem. A native Blish HUD module that receives MIDI input and sends keyboard events directly inside the GW2 overlay would provide a smoother, lower-overhead experience.

## Solution

A Blish HUD module that:

1. Listens to a user-selected MIDI input device via NAudio.
2. Maps incoming MIDI note messages to Guild Wars 2 keyboard keypresses using instrument-specific keymaps.
3. Automatically tracks the current in-game octave and sends octave-shift keys (`9`/`0`) when a note falls outside the current range.
4. Provides a settings tab for device selection, keymap selection, and performance toggles.
5. Exposes a global, configurable keybind to mute/unmute note sending without opening the UI.
6. Indicates active/muted state through a Blish HUD corner icon.

## User Stories

1. As a Guild Wars 2 musician, I want to play in-game instruments with my MIDI keyboard, so that I have a more natural and responsive playing experience.
2. As a Blish HUD user, I want to select my MIDI device from a dropdown in the module settings, so that I can easily switch between controllers.
3. As a performer, I want the module to automatically shift octaves when I play a note outside the current range, so that I don't have to manually tap `9`/`0` while playing.
4. As a performer, I want a configurable global hotkey to toggle note sending on and off, so that I can quickly mute the module if I need to speak in chat or adjust something.
5. As a user, I want a corner icon that changes color when note sending is active or muted, so that I always know the module's current state at a glance.
6. As a user, I want the module to block keypresses when Guild Wars 2 is not in focus, so that I don't accidentally type into Discord or a browser when the module is active.
7. As a user, I want my selected device and keymap to persist across Blish HUD restarts, so that I don't have to reconfigure every time.
8. As a user, I want the module to automatically reconnect if my MIDI device disconnects and comes back (e.g. USB unplug/replug), so that I don't have to manually reselect it.
9. As a user, I want a keymap preview in the settings tab showing the note-to-key mapping, so that I can verify and debug the active configuration.
10. As a user, I want a configurable delay between multi-octave shifts, so that I can tune the responsiveness based on my network latency and GW2 client behavior.
11. As a new user, I want the module to ship with at least one pre-configured instrument keymap (The Minstrel), so that I can start playing immediately without writing a custom configuration.
12. As a user, I want the module to cleanly release all keys when it unloads, so that stuck keys don't persist in GW2 after I close Blish HUD.

## Implementation Decisions

### Module Architecture

Four coordinated modules:

- **MidiInputManager** — encapsulates `NAudio.Midi.MidiIn` lifecycle (open/close/device enumeration) and event queueing. All MIDI messages are captured on a background thread and placed into a `ConcurrentQueue<NoteEvent>`. This is a shallow module: it wraps NAudio and does not contain business logic.
- **KeySender** — the deep module containing all octave-shift logic. It drains the MIDI event queue once per `Module.Update()` tick, looks up notes in the active **Keymap**, resolves **Alt Octave** to avoid unnecessary shifts, and produces a sequence of `SendAction` items (key + optional delay) for the send thread. All this logic is deterministic and side-effect-free.
- **KeymapRegistry** — discovers available **Keymaps**: hardcoded built-ins compiled into the assembly and, in a follow-up, `.json` files from the module's data directory. Provides typed lookup by **Id** with fallback to **Name**.
- **KeySendThread** — a dedicated background thread consuming `SendAction`s from a `BlockingCollection`. It calls `SendInput` for each action and sleeps for configured delays (e.g. multi-octave shift gaps). This isolates timing from the Blish HUD game loop.

### Keymap Data Model

A **Keymap** is identified by a stable `Id` and a display `Name`. It contains a dictionary of `NoteDefinition`s keyed by MIDI note name (e.g. `"C#4"`). Each `NoteDefinition` carries:

- `Key`: the keyboard key to send (e.g. `"1"`, `"f1"`, `"9"`)
- `Octave`: the relative octave this note belongs to
- optionally `AltOctave` + `AltOctaveKey`: an alternate mapping playable from a different octave without shifting
- optionally `ForceInternalOctave`: a "key switch" note that changes the module's internal octave tracker without sending a game key

The **Minstrel (Auto)** built-in keymap is the canonical reference. It includes manual octave-shift fallbacks (`9`/`0`) and force-internal-octave bindings on sharp notes.

### Key Sending

Notes are sent as **KeyTap** — a single `SendInput` call with both `KEYDOWN` and `KEYUP` scan codes back-to-back. No key-hold duration. `noteoff` handling is deferred to a future iteration.

Windows `SendInput` is invoked directly via P/Invoke (`user32.dll`) with scan codes, not virtual keys. This ensures compatibility with GW2's DirectInput layer.

### Threading & Timing

`MidiInputManager` enqueues on a background thread. `Module.Update()` drains to `KeySender`. `KeySender` enqueues `SendAction`s to `KeySendThread`, a separate background sender thread with a `BlockingCollection<SendAction>`.

This model prevents `Thread.Sleep` during octave shifts from stalling the Blish HUD overlay.

### Settings

All settings are registered through Blish HUD's `SettingCollection` in `DefineSettings`:

| Setting | Type | Default |
|---|---|---|
| `SelectedMidiDeviceName` | string | `""` |
| `SelectedKeymapId` | string | `"minstrel-auto"` |
| `SendNotes` | bool | `true` |
| `AutoSwapOctave` | bool | `true` |
| `MultipleOctaveShiftDelay` | int | `75` (ms, range 0–500) |
| `FocusGuard` | bool | `true` |

Standard toggles render automatically. The MIDI device dropdown and keymap dropdown with preview are rendered as a custom inline panel in the settings tab.

### UI

- **Corner Icon** in the Blish HUD icon bar. Active/muted states distinguished by color tint. Click opens the settings tab.
- **Settings Tab** is a single tab with three logical groups (top to bottom): MIDI Device, Keymap + Preview, Standard Controls.
- No floating overlay or note text indicator in v1.

### Error Handling

| Condition | Behavior |
|---|---|
| No MIDI devices | Dropdown shows placeholder; module stays muted |
| Device fails to open | Log warning; auto-reconnect polling begins |
| Device disconnects mid-play | Mute module; start periodic reconnect polling |
| Custom keymap JSON parse fails | Skip file; log warning |
| Module unloads | Send key-up for all possible note/shift keys; stop all threads |

## Testing Decisions

### What makes a good test

Tests exercise external behavior and observable outputs — the sequence of `SendAction`s produced for a given MIDI note stream, or the set of keymaps returned by the registry. They do not test implementation details like private field mutations, exact thread timing, or scan code internals.

### Modules to test

- **KeySender (unit tests, high priority)** — the deepest module. Feed it a series of `NoteOn` events across different octaves with a specific keymap and assert the resulting `SendAction` sequence and final octave state. Exhaustive scenarios:
  - Same octave: single key tap
  - Alt octave resolution: uses alternate key, no octave shift
  - Single octave shift: one `octaveUp`/`octaveDown` key then note
  - Multi-octave shift: multiple shifts with delay between them
  - Non-mapped note: no action
  - Force internal octave: octave changes, no key sent
- **KeymapRegistry (unit tests, high priority)** — assert that built-ins are present, lookup by `Id` succeeds, lookup by `Name` fallback works, and (when implemented) custom JSON files are discovered and parsed.
- **KeySendThread (integration tests, medium priority)** — verify that enqueued actions are dequeued and the thread exits cleanly on shutdown. Testing actual `SendInput` hardware is out of scope.

### Prior art

The project is new; no existing tests. Pattern after standard NUnit/xUnit unit test projects.

## Out of Scope

- Additional built-in keymaps beyond The Minstrel (Auto) — will follow once the data model is validated
- Custom user-defined keymaps via JSON in the data directory
- `noteoff` support / true key-down hold behavior
- Floating overlay indicator or last-note text display
- Chord support (multi-key bindings per note)
- Configuration validation UI for malformed custom keymaps
- Non-Guild-Wars-2 games or platforms

## Further Notes

- The module targets .NET Framework 4.8 to match Blish HUD's runtime.
- NAudio is the only external dependency beyond what Blish HUD already provides.
- `SendInput` targeting the system foreground window is standard behavior, matching the original standalone application. A focus guard setting mitigates accidental keypresses outside GW2.
- The original standalone app used `webmidi` + `robotjs`. The Blish HUD port replaces both with NAudio and raw `SendInput` P/Invoke. The core note-lookup and octave-shift logic is a direct translation of the TypeScript logic.
