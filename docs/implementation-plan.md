# Implementation Plan: Blish HUD MIDI Control

## Phase 1 — Project Scaffold & Dependencies

1. **Update project file** (`Blish HUD - MIDI Control.csproj`) ✅ DONE (namespace, LangVersion 8.0, ref assemblies)
   - Retarget from .NET Framework 4.8 to the Blish HUD compatible version (already 4.8)
   - Remove stale package references (`AsyncClipboardService`, unused SharpDX refs) — pending
   - Add NAudio NuGet package — pending
   - Ensure `Copy Local = False` on all Blish HUD dependencies

2. **Directory structure**
   ```
   src/
   ├── Keymaps/
   │   ├── Keymap.cs / NoteDefinition.cs        ✅ DONE (merged into NoteDefinition.cs)
   │   └── BuiltIn/
   │       └── MinstrelAutoKeymap.cs
   ├── Input/
   │   ├── MidiInputManager.cs
   │   └── SendInput.cs       (P/Invoke wrapper)
   ├── Core/
   │   ├── KeySender.cs
   │   └── KeymapRegistry.cs
   └── UI/
       └── SettingsView.cs
   ```

3. **Cleanup**
   - Retain `Module.cs` as entry point
   - Remove generated `obj/`, `.vs/` cached content

## Phase 2 — Core Domain Classes

### Keymap & Note Model

`NoteDefinition` fields:
- `string Key` — the keyboard key to send (e.g. `"1"`, `"f1"`, `"9"`)
- `int? Octave` — relative octave index (0, 1, 2)
- `int? AltOctave` — alternative octave without shifting
- `string? AltOctaveKey` — key to use when playing from alt octave
- `int? ForceInternalOctave` — sets internal state when played

`Keymap` fields:
- `string Id` — stable identifier (e.g. `"minstrel-auto"`)
- `string Name` — display name
- `bool AutoOctaveSwap`
- `Dictionary<string, NoteDefinition> Notes` — keyed by `"C#4"` etc.
- `string? OctaveDownKey`
- `string? OctaveUpKey`

✅ **Chunk 1 implemented** — `NoteDefinition.cs` with TDD (8 tests passing)

### Built-in Keymap: Minstrel (Auto)

Port the TypeScript data to a static C# class that returns a fully populated `Keymap` instance. All notes, alt-octave definitions, and octave shift bindings.

✅ **Chunk 2 implemented** — `MinstrelAutoKeymap.cs` with TDD (12 tests passing)

### KeymapRegistry

- Exposes `IReadOnlyList<Keymap> AllKeymaps`
- Starts with 1 built-in (Minstrel Auto)
- On load: scan the module data directory (`DirectoriesManager`) for `.json` files matching the keymap schema, deserialize with `System.Text.Json` (or `Newtonsoft.Json` if already available), append to list
- `FindById(string id) → Keymap?` and `FindByName(string name) → Keymap?` for fallback resolution

✅ **Chunk 3 implemented** — `KeymapRegistry.cs` with TDD (6 tests passing)

## Phase 3 — MIDI Input

### MidiInputManager

- `NAudio.Midi.MidiIn` for the currently selected device
- `MessageReceived` fires on background thread → enqueue `MidiEvent` into `ConcurrentQueue<NoteEvent>`
- Expose `IReadOnlyList<string> AvailableDevices` populated by `MidiIn.NumberOfDevices` / `MidiIn.DeviceInfo`
- `Open(string deviceName)` — opens by name, starts `MidiIn`; `Close()` disposes
- If open fails (device in use), surface error via callback/log

### Auto-reconnect

- Background polling task (periodic, e.g. every 3s) triggered when the active device disconnects
- When the device name reappears in `AvailableDevices`, automatically call `Open(name)`

## Phase 4 — Key Sending

### SendInput P/Invoke

```csharp
[DllImport("user32.dll")]
static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
```

`INPUT` struct with `KEYBDINPUT` union. Always send scan codes:
- `dwFlags = KEYEVENTF_SCANCODE` for down
- `dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP` for up

Helper: `SendKeyTap(char key)` → sends down + up in a single `SendInput` call (2 `INPUT` structs).

Helper: `SendKeyUp(char key)` → send key-up for unload safety.

✅ **Chunk 4 implemented** — `SendInput.cs` with TDD (7 tests passing)

### KeyToScanCode

Maps string key names to Win32 scan codes. Used by `KeySender` and `SendInput`.

✅ **Chunk 7 implemented** — `KeyToScanCode.cs` with TDD (13 tests passing)

### KeySendThread

- Dedicated `Thread` with `BlockingCollection<SendAction>`
- `SendAction` = `{ uint ScanCode, int DelayAfterMs }`
- Loop: dequeue, `SendKeyTap(ScanCode)`, if `DelayAfterMs > 0` then `Thread.Sleep(DelayAfterMs)`
- Thread created on `LoadAsync`, joined/disposed on `Unload`

✅ **Chunk 5 implemented** — `KeySendThread.cs` with TDD (6 tests passing)

### KeySender

- Drains `ConcurrentQueue<NoteEvent>` once per `Module.Update()`
- Maintains `int _currentOctave` (starts at 0)
- Pure `Resolve()` function for unit testing: feed `(noteEvent, keymap, currentOctave, autoSwap, shiftDelayMs)` → `(SendAction[], newOctave)`
- `Send()` calls `Resolve()` and enqueues into `KeySendThread`, then updates `_currentOctave`
- Octave shift logic: auto-swap, alt-octave, multi-shift delay
- Manual shift keys (e.g. `C#4` → `"9"`) update octave tracker
- `ForceInternalOctave` notes silently update octave with no keypress
- Focus guard check before every enqueue: if `FocusGuard` setting is enabled and `GameService.GameIntegration.Gw2Instance.IsInGame` is `false`, enqueue nothing.

✅ **Chunk 7 implemented** — `KeySender.cs` with TDD (16 tests passing)

## Phase 6 — Module Shell & Lifecycle

### Module.cs

`DefineSettings` registers:
- `SelectedMidiDeviceName` (string, default "")
- `SelectedKeymapId` (string, default "minstrel-auto")
- `SendNotes` (bool, default true)
- `AutoSwapOctave` (bool, default true)
- `MultipleOctaveShiftDelay` (int, default 75, range 0-500)
- `FocusGuard` (bool, default true)

`Initialize` / `LoadAsync`:
1. Instantiate `KeymapRegistry`
2. Instantiate `MidiInputManager`, attempt to open saved device name
3. Instantiate `KeySender` with registry + queue
4. Instantiate `KeySendThread`, start it
5. Register toggle keybind through `Gw2ApiService.Keyboard.RegisterBinding(...)` — toggles `SendNotes` setting
6. Register corner icon

`Update(GameTime)`:
1. Drain MIDI queue into `KeySender`
2. Check device health, trigger auto-reconnect if needed

`Unload`:
1. Stop `MidiIn`
2. Signal `KeySendThread` to exit, join with timeout
3. Send key-up for all note keys and octave shift keys (`1-8`, `9`, `0`, `f1-f5`) via `SendInput`
4. Dispose all resources

### Corner Icon

- Two states: **active** (green tint) when `SendNotes` is true, **muted** (grey tint) when false
- On click: open the module's settings tab

### Settings Tab UI

Blish HUD `SettingView` with a custom panel. Layout top-to-bottom:

1. **MIDI Device** section
   - Label + `Dropdown` showing `AvailableDevices` (or "No devices found")
   - "Refresh" `StandardButton`
   - On selection change → update `_selectedMidiDeviceName` setting, call `MidiInputManager.Open`

2. **Keymap** section
   - Label + `Dropdown` showing all keymap names
   - On selection change → update `_selectedKeymapId`
   - `TextBox` or multi-line label below showing the selected keymap's notes as formatted text (`C3 → 1 (oct 0)`, `C4 → 1 (oct 1) | alt: 8 on oct 0`, etc.)

3. **Controls** section (standard auto-rendered settings)
   - Checkbox: Send Notes
   - Checkbox: Auto Swap Octave
   - Slider: Multi-Octave Shift Delay (0–500ms)
   - Checkbox: Focus Guard

## Phase 7 — Build & Test

1. Configure `launchSettings.json` pointing to local `Blish HUD.exe`
2. Debug: launch Blish HUD + module with GW2 running
3. Validate:
   - MIDI device appears in dropdown
   - Corner icon toggles with keybind
   - Notes send correctly in Minstrel (Auto) range
   - Auto octave swap fires on boundary notes
   - Multi-octave shift adds delay between `0`/`9` presses
   - Focus guard blocks when GW2 not focused
   - Unload sends key-up safety burst
   - Device disconnect → muted state → reconnect restores

## Follow-up Items (Not in Milestone 1)

- [ ] Additional built-in keymaps (Grand Piano, Flute C/E, Choir Bell, Minstrel non-auto)
- [ ] Custom JSON keymap loading from data directory
- [ ] `noteoff` support / true key-down key-up hold behavior
- [ ] In-overlay indicator showing last played note
- [ ] Chord support for instruments with multi-key bindings
- [ ] Configuration validation and error UI for malformed custom keymaps
