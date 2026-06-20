# Context: Blish HUD MIDI Control

A Blish HUD module that maps MIDI controller input to keyboard keypresses for playing instruments in Guild Wars 2.

## Glossary

| Term | Definition |
|------|------------|
| **Blish HUD** | A .NET Framework overlay framework for Guild Wars 2. The module runs as a separate process and renders over the game window. Chosen over Nexus (a C++ injected proxy) because the module requires direct, low-latency arbitrary key sending (`SendInput`) rather than gamebind-based input. |
| **Keymap** | A mapping from MIDI note names (e.g. `C3`, `D#4`) to GW2 keyboard keys (e.g. `1`, `2`, `9`, `0`), plus octave shift bindings. A keymap also defines which relative octaves each note belongs to, used by the auto-octave-swap feature. |
| **Keybed Layout Preview** | A read-only visual piano-key representation of a keymap's note-to-key mappings, shown on the Keymap Layout settings tab. |
| **Keymap Layout Tab** | A settings tab that hosts the Keybed Layout Preview and allows selection of which keymap to visualize. |
| **Auto Octave Swap** | When enabled, the module tracks the current relative octave. If an incoming MIDI note is in a different octave, the module automatically sends the octave-shift key (`9` or `0`) before sending the note key. A configurable delay is added when shifting multiple octaves to account for GW2 input latency. |
| **Relative Octave** | The currently active octave within a GW2 instrument, represented as a 0-indexed integer (e.g. 0 = lowest, 1 = middle, 2 = highest). The module maintains this state internally. |
| **Alt Octave** | A note definition property specifying an alternative relative octave where the same note can be played without shifting octaves, using a different key. For example, `C4` on octave 1 might also be playable as `8` on octave 0. Used to avoid unnecessary octave shifts. |
| **Built-in Keymap** | A keymap compiled into the module DLL. Cannot be edited by users. Covers known GW2 instruments. |
| **Custom Keymap** | A user-defined keymap stored as a JSON file in the module's data directory. Loaded at runtime and appended to the built-in list. Each keymap has an optional stable `id`; persisted selections prefer `id` and fall back to `name` if no `id` exists. |
| **NAudio** | The selected .NET audio/MIDI library. Provides `MidiIn` for real-time MIDI message capture via the Windows MM API. |
| **Note Event** | A MIDI message indicating a note has been pressed (`noteon`) or released (`noteoff`). The module will initially support `noteon` as key-tap behavior; `noteoff` support will be explored and tested against GW2 instrument response to key-up events. |
| **Focus Guard** | A setting that, when enabled, blocks all key sending when `GameService.GameIntegration.Gw2Instance.IsInGame` is `false`. Prevents accidental keypresses when GW2 is not in focus. |
| **Corner Icon** | A small icon in Blish HUD's top-left corner icon bar indicating module state. Serves as a safety guard to show whether `sendNotes` is active. Color/icon changes based on active/muted state. |
| **KeySendThread** | A dedicated background thread that consumes `SendAction` items from a blocking queue, calls `SendInput` for each, and sleeps for configured delays between actions (e.g., multi-octave shifts). Prevents the Blish HUD game loop from stalling during timed key sequences. |
| **Key Tap** | The default note-to-key behavior: a single `SendInput` call that sends both key-down and key-up events back-to-back (no hold duration). Ignores MIDI note-off events. |
| **Key Switch** | A mapped MIDI note that triggers an internal module action (such as an octave shift) rather than sending a GW2 note key. In a keymap, the note's GW2 binding matches the keymap's `OctaveDownKey` or `OctaveUpKey`. |
| **Key Hold** | An optional note-to-key behavior where a MIDI note-on sends a key-down event and the matching note-off sends a key-up event. The GW2 key is held for the duration the MIDI note is held. |
| **Unload Safety** | When the module unloads, key-up events are sent for all possible octave-shift and note keys to prevent stuck key states in GW2. |
| **MidiInputManager** | Manages NAudio `MidiIn` lifecycle: opens the selected device, registers `MessageReceived`, and buffers events into a `ConcurrentQueue`. All MIDI events are enqueued on a background thread; the queue is drained once per `Module.Update()`. |
| **KeySender** | Consumes buffered MIDI events from `MidiInputManager`, runs octave-shift logic using the active `Keymap`, and sends keyboard events via P/Invoke `SendInput`. |
| **KeymapRegistry** | Provides access to all available keymaps: built-in hardcoded C# classes plus any custom JSON files loaded from the module's data directory. |
| **Toggle Keybind** | A user-configurable keybind (registered through Blish HUD's global keybind system) that toggles `sendNotes` between on/off. No fixed default. |
| **SendInput** | The Win32 API (`user32.dll`) used for sending keyboard scan codes directly to the system. Chosen over wrapper libraries because the module only needs single-key tap/hold; raw P/Invoke is minimal and sufficient. |
