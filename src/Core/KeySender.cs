#nullable enable

using System;
using System.Collections.Generic;
using DavidRice.BlishHud.MidiControl.Input;

namespace DavidRice.BlishHud.MidiControl.Core
{
    public readonly struct KeySendResult
    {
        public SendAction[] Actions { get; }
        public int NewOctave { get; }
        public int PreviousOctave { get; }
        public string[] SentKeyNames { get; }
        public bool WasSuppressed { get; }

        public KeySendResult(SendAction[] actions, int newOctave, int previousOctave, string[] sentKeyNames, bool wasSuppressed = false)
        {
            Actions = actions;
            NewOctave = newOctave;
            PreviousOctave = previousOctave;
            SentKeyNames = sentKeyNames;
            WasSuppressed = wasSuppressed;
        }
    }

    /// <summary>
    /// Consumes <see cref="MidiNoteEvent"/>s, resolves octave-shift logic using the active
    /// <see cref="Keymap"/>, and enqueues <see cref="SendAction"/>s into <see cref="KeySendThread"/>.
    /// </summary>
    public class KeySender
    {
        private readonly KeySendThread _keySendThread;
        private readonly Dictionary<uint, int> _heldKeys = new Dictionary<uint, int>();
        private readonly Dictionary<int, uint> _noteToScanCode = new Dictionary<int, uint>();
        private int _currentOctave;

        public KeySender(KeySendThread keySendThread)
        {
            _keySendThread = keySendThread ?? throw new ArgumentNullException(nameof(keySendThread));
        }

        public int CurrentOctave => _currentOctave;

        /// <summary>
        /// Fires after a note has been resolved and its <see cref="SendAction"/>s have been enqueued.
        /// Useful for logging and diagnostics.
        /// </summary>
        public event Action<MidiNoteEvent, KeySendResult>? NoteProcessed;

        /// <summary>
        /// Processes a single MIDI note event, applying the active keymap and current octave state.
        /// <see cref="SendAction"/>s are enqueued immediately into the backing <see cref="KeySendThread"/>.
        /// </summary>
        public void Send(MidiNoteEvent noteEvent, Keymap keymap, bool autoSwap, int shiftDelayMs, bool enableKeyHold = false)
        {
            if (enableKeyHold && !noteEvent.IsNoteOn)
            {
                ProcessKeyHoldNoteOff(noteEvent, keymap);
                return;
            }

            var candidate = Resolve(noteEvent, keymap, _currentOctave, autoSwap, shiftDelayMs, enableKeyHold);

            // In Key Hold mode, remember which physical key was pressed for this note so a later
            // note-off can release the same scan code even if the octave has changed in between.
            if (enableKeyHold && candidate.Actions.Length > 0)
            {
                int noteActionIndex = candidate.Actions.Length - 1;
                _noteToScanCode[noteEvent.NoteNumber] = candidate.Actions[noteActionIndex].ScanCode;
            }

            var actualActions = new List<SendAction>(candidate.Actions.Length);
            var actualSentKeyNames = new List<string>(candidate.SentKeyNames.Length);
            bool wasSuppressed = false;

            for (int i = 0; i < candidate.Actions.Length; i++)
            {
                var action = candidate.Actions[i];
                string keyName = candidate.SentKeyNames[i];

                switch (action.EventType)
                {
                    case KeyEventType.KeyTap:
                        actualActions.Add(action);
                        actualSentKeyNames.Add(keyName);
                        break;

                    case KeyEventType.KeyDown:
                        _heldKeys.TryGetValue(action.ScanCode, out int previousCount);
                        _heldKeys[action.ScanCode] = previousCount + 1;

                        if (previousCount == 0)
                        {
                            actualActions.Add(action);
                            actualSentKeyNames.Add(keyName);
                        }
                        else
                        {
                            wasSuppressed = true;
                        }
                        break;

                    case KeyEventType.KeyUp:
                        if (TryReleaseHeldScanCode(action.ScanCode, out bool released))
                        {
                            actualActions.Add(action);
                            actualSentKeyNames.Add(keyName);
                        }
                        else if (!released)
                        {
                            // Key is not tracked (missed note-on). Send key-up anyway to avoid a
                            // stuck key in the game.
                            actualActions.Add(action);
                            actualSentKeyNames.Add(keyName);
                        }
                        else
                        {
                            wasSuppressed = true;
                        }
                        break;
                }
            }

            var result = new KeySendResult(
                actualActions.ToArray(),
                candidate.NewOctave,
                candidate.PreviousOctave,
                actualSentKeyNames.ToArray(),
                wasSuppressed);

            foreach (var action in result.Actions)
            {
                _keySendThread.Enqueue(action);
            }

            NoteProcessed?.Invoke(noteEvent, result);
            _currentOctave = candidate.NewOctave;
        }

        /// <summary>
        /// Releases every scan code currently tracked as held and clears the trackers.
        /// </summary>
        public void ReleaseAllHeldKeys()
        {
            foreach (var kvp in _heldKeys)
            {
                _keySendThread.Enqueue(new SendAction(kvp.Key, eventType: KeyEventType.KeyUp));
            }

            _heldKeys.Clear();
            _noteToScanCode.Clear();
        }

        private void ProcessKeyHoldNoteOff(MidiNoteEvent noteEvent, Keymap keymap)
        {
            uint scanCode;
            string keyName;

            if (_noteToScanCode.TryGetValue(noteEvent.NoteNumber, out scanCode))
            {
                keyName = KeyToScanCode.GetKeyName(scanCode) ?? "?";
            }
            else
            {
                // We never saw the matching note-on (lost message, tracker cleared, etc.).
                // Resolve without auto-swap as a best-effort guess so we still try to release a key.
                var fallback = Resolve(noteEvent, keymap, _currentOctave, autoSwap: false, shiftDelayMs: 0, enableKeyHold: true);
                if (fallback.Actions.Length == 0)
                {
                    NoteProcessed?.Invoke(noteEvent, new KeySendResult(
                        Array.Empty<SendAction>(),
                        _currentOctave,
                        _currentOctave,
                        Array.Empty<string>()));
                    return;
                }

                scanCode = fallback.Actions[0].ScanCode;
                keyName = fallback.SentKeyNames[0];
            }

            var actualActions = new List<SendAction>();
            var actualSentKeyNames = new List<string>();
            bool wasSuppressed = false;

            if (TryReleaseHeldScanCode(scanCode, out bool released))
            {
                actualActions.Add(new SendAction(scanCode, eventType: KeyEventType.KeyUp));
                actualSentKeyNames.Add(keyName);
            }
            else if (!released)
            {
                // Untracked key-up: send anyway to avoid a stuck key.
                actualActions.Add(new SendAction(scanCode, eventType: KeyEventType.KeyUp));
                actualSentKeyNames.Add(keyName);
            }
            else
            {
                wasSuppressed = true;
            }

            if (_heldKeys.Count == 0 || !_heldKeys.ContainsKey(scanCode))
            {
                _noteToScanCode.Remove(noteEvent.NoteNumber);
            }

            var result = new KeySendResult(
                actualActions.ToArray(),
                _currentOctave,
                _currentOctave,
                actualSentKeyNames.ToArray(),
                wasSuppressed);

            foreach (var action in result.Actions)
            {
                _keySendThread.Enqueue(action);
            }

            NoteProcessed?.Invoke(noteEvent, result);
        }

        /// <summary>
        /// Decrements the refcount for a held scan code.
        /// Returns true if the key should be released (refcount reached zero),
        /// false otherwise. <paramref name="released"/> is true when the scan code was tracked,
        /// false if it was never held.
        /// </summary>
        private bool TryReleaseHeldScanCode(uint scanCode, out bool released)
        {
            if (_heldKeys.TryGetValue(scanCode, out int heldCount))
            {
                released = true;
                int newCount = heldCount - 1;
                if (newCount == 0)
                {
                    _heldKeys.Remove(scanCode);
                    return true;
                }

                _heldKeys[scanCode] = newCount;
                return false;
            }

            released = false;
            return false;
        }

        /// <summary>
        /// Pure function that decides which <see cref="SendAction"/>s to produce for a note event
        /// and what the new octave state should be.  Does not perform I/O.
        /// </summary>
        public static KeySendResult Resolve(
            MidiNoteEvent noteEvent,
            Keymap keymap,
            int currentOctave,
            bool autoSwap,
            int shiftDelayMs,
            bool enableKeyHold = false)
        {
            KeyEventType eventType;
            if (enableKeyHold)
            {
                eventType = noteEvent.IsNoteOn ? KeyEventType.KeyDown : KeyEventType.KeyUp;
            }
            else if (noteEvent.IsNoteOn)
            {
                eventType = KeyEventType.KeyTap;
            }
            else
            {
                // Key Tap mode ignores note-off events.
                return new KeySendResult(Array.Empty<SendAction>(), currentOctave, currentOctave, Array.Empty<string>());
            }

            string noteName = MidiNote.GetNoteName(noteEvent.NoteNumber);

            if (!keymap.Notes.TryGetValue(noteName, out var definition))
                return new KeySendResult(Array.Empty<SendAction>(), currentOctave, currentOctave, Array.Empty<string>());

            if (definition.ForceInternalOctave.HasValue)
                return new KeySendResult(Array.Empty<SendAction>(), definition.ForceInternalOctave.Value, currentOctave, Array.Empty<string>());

            if (definition.Key == null)
                return new KeySendResult(Array.Empty<SendAction>(), currentOctave, currentOctave, Array.Empty<string>());

            string noteKey = definition.Key;
            var actions = new List<SendAction>();
            int newOctave = currentOctave;

            // Note definition with a key but no octave is a special key (e.g. manual octave shift).
            if (!definition.Octave.HasValue)
            {
                uint? sc = KeyToScanCode.For(noteKey);
                var specialKeyNames = new List<string>();
                if (sc.HasValue)
                {
                    actions.Add(new SendAction(sc.Value, eventType: eventType));
                    string? keyName = KeyToScanCode.GetKeyName(sc.Value) ?? noteKey;
                    specialKeyNames.Add(keyName);
                }

                if (noteEvent.IsNoteOn)
                {
                    if (keymap.OctaveDownKey != null &&
                        noteKey.Equals(keymap.OctaveDownKey, StringComparison.OrdinalIgnoreCase))
                    {
                        newOctave = Math.Max(0, currentOctave - 1);
                    }
                    else if (keymap.OctaveUpKey != null &&
                             noteKey.Equals(keymap.OctaveUpKey, StringComparison.OrdinalIgnoreCase))
                    {
                        newOctave = currentOctave + 1;
                    }
                }

                return new KeySendResult(actions.ToArray(), newOctave, currentOctave, specialKeyNames.ToArray());
            }

            int targetOctave = definition.Octave.Value;
            string targetKey = definition.Key!;
            var shiftKeyNames = new List<string>();

            if (autoSwap && currentOctave != targetOctave)
            {
                bool canUseAlt = definition.AltOctave.HasValue
                    && definition.AltOctave.Value == currentOctave
                    && definition.AltOctaveKey != null;

                if (canUseAlt)
                {
                    targetOctave = currentOctave;
                    targetKey = definition.AltOctaveKey!;
                }
                else
                {
                    int octavesToShift = targetOctave - currentOctave;
                    if (!TryBuildOctaveShiftActions(keymap, octavesToShift, shiftDelayMs, actions, shiftKeyNames))
                        return new KeySendResult(Array.Empty<SendAction>(), currentOctave, currentOctave, Array.Empty<string>());

                    newOctave = targetOctave;
                }
            }

            uint? noteSc = KeyToScanCode.For(targetKey);
            if (noteSc.HasValue)
            {
                actions.Add(new SendAction(noteSc.Value, eventType: eventType));

                var allKeyNames = new List<string>(shiftKeyNames.Count);
                allKeyNames.AddRange(shiftKeyNames);
                string? noteKeyName = KeyToScanCode.GetKeyName(noteSc.Value) ?? targetKey;
                allKeyNames.Add(noteKeyName);

                return new KeySendResult(actions.ToArray(), newOctave, currentOctave, allKeyNames.ToArray());
            }

            return new KeySendResult(Array.Empty<SendAction>(), currentOctave, currentOctave, Array.Empty<string>());
        }

        private static bool TryBuildOctaveShiftActions(
            Keymap keymap,
            int octavesToShift,
            int shiftDelayMs,
            List<SendAction> actions,
            List<string> keyNames)
        {
            if (octavesToShift > 0)
            {
                string? upKey = keymap.OctaveUpKey;
                if (string.IsNullOrEmpty(upKey))
                    return false;

                uint? sc = KeyToScanCode.For(upKey!);
                if (!sc.HasValue)
                    return false;

                string? keyName = KeyToScanCode.GetKeyName(sc.Value);
                AppendShiftActions(sc.Value, octavesToShift, shiftDelayMs, actions, keyName);
                if (keyName != null)
                {
                    for (int i = 0; i < octavesToShift; i++)
                        keyNames.Add(keyName);
                }
                return true;
            }

            if (octavesToShift < 0)
            {
                string? downKey = keymap.OctaveDownKey;
                if (string.IsNullOrEmpty(downKey))
                    return false;

                uint? sc = KeyToScanCode.For(downKey!);
                if (!sc.HasValue)
                    return false;

                string? keyName = KeyToScanCode.GetKeyName(sc.Value);
                AppendShiftActions(sc.Value, Math.Abs(octavesToShift), shiftDelayMs, actions, keyName);
                if (keyName != null)
                {
                    for (int i = 0; i < Math.Abs(octavesToShift); i++)
                        keyNames.Add(keyName);
                }
                return true;
            }

            return true;
        }

        private static void AppendShiftActions(
            uint scanCode,
            int shiftCount,
            int shiftDelayMs,
            List<SendAction> actions,
            string? keyName)
        {
            for (int i = 0; i < shiftCount; i++)
            {
                bool isLast = i == shiftCount - 1;
                int delay = isLast ? 0 : shiftDelayMs;
                actions.Add(new SendAction(scanCode, delay));
            }
        }
    }
}
