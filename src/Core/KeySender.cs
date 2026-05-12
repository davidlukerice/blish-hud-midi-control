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

        public KeySendResult(SendAction[] actions, int newOctave)
        {
            Actions = actions;
            NewOctave = newOctave;
        }
    }

    /// <summary>
    /// Consumes <see cref="MidiNoteEvent"/>s, resolves octave-shift logic using the active
    /// <see cref="Keymap"/>, and enqueues <see cref="SendAction"/>s into <see cref="KeySendThread"/>.
    /// </summary>
    public class KeySender
    {
        private readonly KeySendThread _keySendThread;
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
        public void Send(MidiNoteEvent noteEvent, Keymap keymap, bool autoSwap, int shiftDelayMs)
        {
            var result = Resolve(noteEvent, keymap, _currentOctave, autoSwap, shiftDelayMs);
            foreach (var action in result.Actions)
            {
                _keySendThread.Enqueue(action);
            }
            NoteProcessed?.Invoke(noteEvent, result);
            _currentOctave = result.NewOctave;
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
            int shiftDelayMs)
        {
            if (!noteEvent.IsNoteOn)
                return new KeySendResult(Array.Empty<SendAction>(), currentOctave);

            string noteName = MidiNote.GetNoteName(noteEvent.NoteNumber);

            if (!keymap.Notes.TryGetValue(noteName, out var definition))
                return new KeySendResult(Array.Empty<SendAction>(), currentOctave);

            if (definition.ForceInternalOctave.HasValue)
                return new KeySendResult(Array.Empty<SendAction>(), definition.ForceInternalOctave.Value);

            if (definition.Key == null)
                return new KeySendResult(Array.Empty<SendAction>(), currentOctave);

            string noteKey = definition.Key;
            var actions = new List<SendAction>();
            int newOctave = currentOctave;

            // Note definition with a key but no octave is a special key (e.g. manual octave shift).
            if (!definition.Octave.HasValue)
            {
                uint? sc = KeyToScanCode.For(noteKey);
                if (sc.HasValue)
                    actions.Add(new SendAction(sc.Value));

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

                return new KeySendResult(actions.ToArray(), newOctave);
            }

            int targetOctave = definition.Octave.Value;
            string targetKey = definition.Key!;

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
                    if (!TryBuildOctaveShiftActions(keymap, octavesToShift, shiftDelayMs, actions))
                        return new KeySendResult(Array.Empty<SendAction>(), currentOctave);

                    newOctave = targetOctave;
                }
            }

            uint? noteSc = KeyToScanCode.For(targetKey);
            if (noteSc.HasValue)
                actions.Add(new SendAction(noteSc.Value));

            return new KeySendResult(actions.ToArray(), newOctave);
        }

        private static bool TryBuildOctaveShiftActions(
            Keymap keymap,
            int octavesToShift,
            int shiftDelayMs,
            List<SendAction> actions)
        {
            if (octavesToShift > 0)
            {
                string? upKey = keymap.OctaveUpKey;
                if (string.IsNullOrEmpty(upKey))
                    return false;

                uint? sc = KeyToScanCode.For(upKey!);
                if (!sc.HasValue)
                    return false;

                AppendShiftActions(sc.Value, octavesToShift, shiftDelayMs, actions);
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

                AppendShiftActions(sc.Value, Math.Abs(octavesToShift), shiftDelayMs, actions);
                return true;
            }

            return true;
        }

        private static void AppendShiftActions(
            uint scanCode,
            int shiftCount,
            int shiftDelayMs,
            List<SendAction> actions)
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
