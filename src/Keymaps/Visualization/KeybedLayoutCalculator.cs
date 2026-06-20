#nullable enable

using System.Collections.Generic;
using DavidRice.BlishHud.MidiControl.Core;

namespace DavidRice.BlishHud.MidiControl.Keymaps.Visualization
{
    public static class KeybedLayoutCalculator
    {
        private static readonly int[] BlackKeySemitones = { 1, 3, 6, 8, 10 };

        public static KeybedLayout Calculate(Keymap keymap)
        {
            var mappedNotes = new List<(string NoteName, int NoteNumber, NoteDefinition Definition)>();

            foreach (var pair in keymap.Notes)
            {
                if (string.IsNullOrWhiteSpace(pair.Value.Key))
                    continue;

                if (!MidiNote.TryParseNoteName(pair.Key, out int noteNumber))
                    continue;

                mappedNotes.Add((pair.Key, noteNumber, pair.Value));
            }

            if (mappedNotes.Count == 0)
                return KeybedLayout.Empty;

            int minNoteNumber = mappedNotes[0].NoteNumber;
            int maxNoteNumber = mappedNotes[0].NoteNumber;

            foreach (var mapped in mappedNotes)
            {
                if (mapped.NoteNumber < minNoteNumber)
                    minNoteNumber = mapped.NoteNumber;
                if (mapped.NoteNumber > maxNoteNumber)
                    maxNoteNumber = mapped.NoteNumber;
            }

            int startOctave = minNoteNumber / 12 - 1;
            int endOctave = maxNoteNumber / 12 - 1;

            int startNote = (startOctave + 1) * 12;
            int endNote = (endOctave + 1) * 12 + 11;

            var mappedByNoteNumber = new Dictionary<int, (string NoteName, NoteDefinition Definition)>();
            foreach (var mapped in mappedNotes)
            {
                mappedByNoteNumber[mapped.NoteNumber] = (mapped.NoteName, mapped.Definition);
            }

            var keys = new List<KeybedKey>();
            for (int note = startNote; note <= endNote; note++)
            {
                bool isBlackKey = IsBlackKey(note);

                bool isMapped = mappedByNoteNumber.TryGetValue(note, out var mappedEntry);
                string noteName = isMapped ? mappedEntry.NoteName : MidiNote.GetNoteName(note);
                string? gw2Key = isMapped ? mappedEntry.Definition.Key : null;
                int? octave = isMapped ? mappedEntry.Definition.Octave : null;
                bool hasAltOctave = isMapped && mappedEntry.Definition.AltOctave.HasValue;
                int? altOctave = hasAltOctave ? mappedEntry.Definition.AltOctave : null;
                string? altOctaveKey = hasAltOctave ? mappedEntry.Definition.AltOctaveKey : null;

                bool isKeySwitch = isMapped &&
                    (!string.IsNullOrWhiteSpace(keymap.OctaveDownKey) && gw2Key == keymap.OctaveDownKey ||
                     !string.IsNullOrWhiteSpace(keymap.OctaveUpKey) && gw2Key == keymap.OctaveUpKey);

                keys.Add(new KeybedKey(
                    note,
                    noteName,
                    isBlackKey,
                    isMapped,
                    gw2Key,
                    isKeySwitch,
                    octave,
                    hasAltOctave,
                    altOctave,
                    altOctaveKey));
            }

            return new KeybedLayout(keys, startOctave, endOctave);
        }

        private static bool IsBlackKey(int noteNumber)
        {
            int semitone = noteNumber % 12;
            foreach (int blackSemitone in BlackKeySemitones)
            {
                if (semitone == blackSemitone)
                    return true;
            }

            return false;
        }
    }
}
