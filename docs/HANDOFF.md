# Agent Handoff — Blish HUD MIDI Control

## Completed Work (Ready)

Chunks 1–8 plus settings UI are fully implemented and unit-tested (76 tests passing):
- Domain model: `NoteDefinition`, `Keymap`
- Built-in keymap: `MinstrelAutoKeymap`
- `KeymapRegistry`
- `SendInput` P/Invoke wrapper
- `KeyToScanCode` mapping utility
- `KeySendThread`
- `MidiInputManager`, `MidiNote`, `MidiNoteEvent`
- `KeySender` (pure `Resolve()` + `Send()`) with `NoteProcessed` event for diagnostics
- **`KeySender` wired into `Module.cs`** — `Update()` drains queue, respects settings, keymap fallback
- **`MidiSettingsView`** — custom settings panel with device dropdown, keymap dropdown, toggles, delay slider, and recent-send log
- **`TabbedWindow` settings** — opens on corner icon click
- **Blish HUD 1.3.0 upgrade** — package refs updated, breaking API fixes applied, builds cleanly

## Remaining Work

| # | Name | Files | Verifiable because |
|---|---|---|---|
| 9 | **Auto-reconnect** | `Module.cs`, `MidiInputManager.cs?` | Simulated by force-closing device in unit test or observable log output |
| 10 | **Toggle keybind** | `Module.cs` | Keybind registration compiles; integration tested at runtime |
| 13 | **Build/package** | `.csproj`, post-build | Clean build, `.bhm` post-build xcopy restored, stale packages removed |
| **14** | **1.3.0 deprecation cleanup** | `Module.cs`, `MidiSettingsView.cs` | Zero `CS0618` warnings on build |
| **15** | **Fix MIDI device selection** | `MidiSettingsView.cs`, `Module.cs` | Dropdown shows devices, selecting one opens the device and updates status label |

## Important Notes

- **Blish HUD upgraded to v1.3.0** — `packages.config` + `.csproj` references updated. Two breaking API changes were fixed:
  - `GameService.Graphics.GraphicsDevice` → `GameService.Graphics.LendGraphicsDeviceContext()`
  - `GameService.GameIntegration.Gw2IsRunning` → `GameService.GameIntegration.Gw2Instance.Gw2IsRunning`
- **Deprecations to address** (non-breaking but noisy):
  - `TabbedWindow` → `TabbedWindow2`
  - `DefineSetting<TEntry>(...)` → localization-friendly overload
  - `TabbedWindow.AddTab(string, AsyncTexture2D, Panel)` → pass `Func<IView>` instead
- **Corner icon click opens the settings window** instead of toggling `SendNotes`. The mute toggle is available as a checkbox inside the settings panel.
- **`KeySender.NoteProcessed`** fires after each note is resolved and enqueued. The handler in `Module.cs` pushes a formatted entry into `_recentSendLog` (max 10 entries).
- **Settings window content** (`MidiSettingsView`) uses absolute positioning (`Location = new Point(x, y)`) with a vertical `y` tracker. This is the safest layout approach for Blish HUD's control system.

## Next Chunk

**Chunk 15: Fix MIDI device selection in the settings UI.**
The dropdown populates but selecting a device does not appear to actually open it. Need to verify the `ValueChanged` handler, confirm `_module.OpenMidiDevice()` is called, and ensure the status label updates. Once confirmed working, proceed to **Chunk 14: 1.3.0 deprecation cleanup** to eliminate all `CS0618` warnings.
