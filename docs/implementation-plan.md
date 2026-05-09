# Implementation Plan: Blish HUD MIDI Control

## Phase 1 — Project Scaffold & Dependencies

1. **Update project file** (`Blish HUD - MIDI Control.csproj`)
   - Retarget from .NET Framework 4.8 to the Blish HUD compatible version (already 4.8)
   - Remove stale package references (`AsyncClipboardService`, unused SharpDX refs)
   - Add NAudio NuGet package
   - Ensure `Copy Local = False` on all Blish HUD dependencies

2. **Directory structure**
   ```
   src/
   ├── Keymaps/
   │   ├── Keymap.cs          (data model)
   │   ├── NoteDefinition.cs  (single note mapping)
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

### Built-in Keymap: Minstrel (Auto)

Port the TypeScript data to a static C# class that returns a fully populated `Keymap` instance. All notes, alt-octave definitions, and octave shift bindings.

### KeymapRegistry

- Exposes `IReadOnlyList<Keymap> AllKeymaps`
- Starts with 1 built-in (Minstrel Auto)
- On load: scan the module data directory (`DirectoriesManager`) for `.json` files matching the keymap schema, deserialize with `System.Text.Json` (or `Newtonsoft.Json` if already available), append to list
- `FindById(string id) → Keymap?` and `FindByName(string name) → Keymap?` for fallback resolution

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

### KeySendThread

- Dedicated `Thread` with `BlockingCollection<SendAction>`
- `SendAction` = `{ char Key, int DelayAfterMs }`
- Loop: dequeue, `SendKeyTap(Key)`, if `DelayAfterMs > 0` then `Thread.Sleep(DelayAfterMs)`
- Thread created on `LoadAsync`, joined/disposed on `Unload`

## Phase 5 — KeySender (Business Logic)

- Drains `ConcurrentQueue<NoteEvent>` once per `Module.Update()`
- Maintains `int _currentOctave` (starts at 1 for Minstrel)
- For each `noteon`:
  1. Look up note in active `Keymap`
  2. If not found, log (debug builds only) and skip
  3. If `AutoOctaveSwap` is on, compare note octave to `_currentOctave`
  4. If `AltOctave` matches `_currentOctave`, use `AltOctaveKey` (no shift needed)
  5. Otherwise, enqueue octave shifts (`9`/`0`) with delay if multi-octave
  6. Enqueue the note key with `DelayAfterMs = 0`
  7. Update `_currentOctave`

Focus guard check before every enqueue: if `FocusGuard` setting is enabled and `GameService.GameIntegration.Gw2Instance.IsInGame` is `false`, enqueue nothing.

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
