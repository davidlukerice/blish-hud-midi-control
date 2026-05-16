# Agent Handoff — Blish HUD MIDI Control

## Completed Work (Ready)

Chunks 1–9, 13–15 plus settings UI are fully implemented and unit-tested (84 tests passing):
- Domain model: `NoteDefinition`, `Keymap`
- Built-in keymap: `MinstrelAutoKeymap`
- `KeymapRegistry`
- `SendInput` P/Invoke wrapper
- `KeyToScanCode` mapping utility
- `KeySendThread`
- `MidiInputManager`, `MidiNote`, `MidiNoteEvent`
- `KeySender` (pure `Resolve()` + `Send()`) with `NoteProcessed` event for diagnostics
- `ConnectionEvaluator` — pure function for device connect/disconnect/reconnect logic
- **Auto-reconnect** — `MidiInputManager.CheckConnection()` polls `AvailableDevices` every 10s, closes dead handles, reopens when device reappears; `MidiDeviceStatus` shows "Disconnected — retrying" state; status label in settings UI updates live each frame
- **Corner icon states** — green (active), gray (muted), orange (disconnected/retrying). Icon and tooltip update automatically on disconnect/reconnect and on Send Notes toggle.
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
| 10 | **Toggle keybind** | `Module.cs` | Keybind registration compiles; integration tested at runtime |

## Important Notes

- **Blish HUD upgraded to v1.3.0** — `packages.config` + `.csproj` references updated.
- **Corner icon click opens the settings window** instead of toggling `SendNotes`. The mute toggle is available as a checkbox inside the settings panel.
- **`KeySender.NoteProcessed`** fires after each note is resolved and enqueued. The handler in `Module.cs` pushes a formatted entry into `_recentSendLog` (max 10 entries).
- **Settings window** uses `TabbedWindow2` with asset 155997. The `DoBuild` receiver is a `TabbedWindow2` (not a `Panel`), so `MidiSettingsTabView` creates a child `Panel` before calling `MidiSettingsView.Build()`.
- **Content offset**: `MidiSettingsView.Build()` uses `x=95, y=40` starting position to account for the tab sidebar (~82px) and window chrome (~30px) in the full-window container.
- **Auto-device-open**: After `RefreshDevices()` reattaches the `ValueChanged` handler, it manually calls `OnDeviceSelected` to open the initially-selected device (since `SelectedItem` was set while the handler was detached).

## Next Chunk

**Chunk 10: Toggle keybind**

## Chunk 13 Details

- **Stale packages removed**: 25 stale package directories deleted from `packages/` (old BlishHUD 0.5.2, MonoGame 3.7.x, NAudio 2.0–2.2.x, Newtonsoft 12, Gw2Sharp 0.10, etc.)
- **`packages.config` cleaned**: removed 4 dead entries (`NAudio.Wasapi`, `System.ComponentModel.Composition`, `System.Resources.Extensions`, `System.ServiceModel.Primitives`); added `NAudio.Core 2.3.0` and `NAudio.Midi 2.3.0`
- **NAudio references fixed**: `.csproj` paths changed from absolute `~/.nuget` paths to relative `packages/` paths (works on other machines)
- **Post-build xcopy restored**: MSBuild target `CopyBhmToModules` runs after `BuildBlishHUDModule`, copies `.bhm` to `%USERPROFILE%\Documents\Guild Wars 2\addons\blishhud\modules\`

## Chunk 9 Details

- `ConnectionEvaluator` — pure static `Evaluate(targetName, availableDevices, isOpen)` returns `NoAction/Close/Reopen`; testable without Blish HUD runtime
- `MidiInputManager.CheckConnection(targetName)` — throttled to every 10s via `_lastConnectionCheck`; calls `Evaluate`, acts on result, sets `IsRetryingConnection` flag
- `MidiInputManager.IsRetryingConnection` — `true` after disconnect detected, reset to `false` on `Open()` or `Dispose()`
- `Module.Update()` — calls `CheckConnection` each frame; updates `StatusLabel` text; tracks `_wasRetrying` to refresh corner icon on state change
- `MidiModule.StatusLabel` — internal setter that `MidiSettingsView` wires to its `_statusLabel` during `Build()`; nulled in `Unload()`
- Corner icon: `_disconnectedIconTexture` (orange 16×16) added to `CreateCornerIcon()` and `Unload()`
- Tests: `tests/Core/MidiInputManagerTests.cs` — 8 tests covering all `ConnectionEvaluator.Evaluate` state transitions
