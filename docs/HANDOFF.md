# Agent Handoff — Blish HUD MIDI Control

## Completed Work (Ready)

Chunks 1–8, 14, and 15 plus settings UI are fully implemented and unit-tested (76 tests passing):
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
- **`TabbedWindow2` settings** — opens on corner icon click, renders correctly with content offset
- **Blish HUD 1.3.0 upgrade** — package refs updated, breaking API fixes applied, builds cleanly
- **1.3.0 deprecation cleanup** — zero `CS0618` warnings
  - `TabbedWindow` → `TabbedWindow2` with `AsyncTexture2D.FromAssetId(155997)` background
  - `DefineSetting<T>(key, val, "name", "desc")` → `DefineSetting<T>(key, val, () => "name", () => "desc")`
  - `AddTab(name, icon, Panel)` → `Tabs.Add(new Tab(icon, Func<IView>, name))` via `MidiSettingsTabView : IView`
- **MIDI device selection fixed** — dropdown populates, auto-selects and opens device on first load, status label updates

## Remaining Work

| # | Name | Files | Verifiable because |
|---|---|---|---|
| 9 | **Auto-reconnect** | `Module.cs`, `MidiInputManager.cs?` | Simulated by force-closing device in unit test or observable log output |
| 10 | **Toggle keybind** | `Module.cs` | Keybind registration compiles; integration tested at runtime |
| 13 | **Build/package** | `.csproj`, post-build | Clean build, `.bhm` post-build xcopy restored, stale packages removed |

## Important Notes

- **Blish HUD upgraded to v1.3.0** — `packages.config` + `.csproj` references updated.
- **Corner icon click opens the settings window** instead of toggling `SendNotes`. The mute toggle is available as a checkbox inside the settings panel.
- **`KeySender.NoteProcessed`** fires after each note is resolved and enqueued. The handler in `Module.cs` pushes a formatted entry into `_recentSendLog` (max 10 entries).
- **Settings window** uses `TabbedWindow2` with asset 155997. The `DoBuild` receiver is a `TabbedWindow2` (not a `Panel`), so `MidiSettingsTabView` creates a child `Panel` before calling `MidiSettingsView.Build()`.
- **Content offset**: `MidiSettingsView.Build()` uses `x=95, y=40` starting position to account for the tab sidebar (~82px) and window chrome (~30px) in the full-window container.
- **Auto-device-open**: After `RefreshDevices()` reattaches the `ValueChanged` handler, it manually calls `OnDeviceSelected` to open the initially-selected device (since `SelectedItem` was set while the handler was detached).

## Next Chunk

**Chunk 9: Auto-reconnect**, **Chunk 10: Toggle keybind**, or **Chunk 13: Build/package** — whichever you prefer.
