#nullable enable

using System;

namespace DavidRice.BlishHud.MidiControl.Core
{
    public static class MidiNote
    {
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string GetNoteName(int noteNumber)
        {
            string noteName = NoteNames[noteNumber % 12];
            int octave = noteNumber / 12 - 1;
            return $"{noteName}{octave}";
        }

        public static bool TryParseNoteName(string? noteName, out int noteNumber)
        {
            noteNumber = 0;

            if (string.IsNullOrWhiteSpace(noteName))
                return false;

            noteName = noteName!.Trim();

            if (noteName.Length < 2)
                return false;

            char letter = char.ToUpperInvariant(noteName[0]);
            if (letter < 'A' || letter > 'G')
                return false;

            int accidentalOffset = 0;
            int octaveStartIndex = 1;

            if (noteName.Length > 1)
            {
                char accidental = noteName[1];
                if (accidental == '#')
                {
                    accidentalOffset = 1;
                    octaveStartIndex = 2;
                }
                else if (accidental == 'b' || accidental == 'B')
                {
                    accidentalOffset = -1;
                    octaveStartIndex = 2;
                }
            }

            if (octaveStartIndex >= noteName.Length)
                return false;

            string octavePart = noteName.Substring(octaveStartIndex);
            if (!int.TryParse(octavePart, out int octave))
                return false;

            int baseOffset = letter switch
            {
                'C' => 0,
                'D' => 2,
                'E' => 4,
                'F' => 5,
                'G' => 7,
                'A' => 9,
                'B' => 11,
                _ => throw new InvalidOperationException("Unexpected note letter.")
            };

            int semitone = baseOffset + accidentalOffset;
            noteNumber = (octave + 1) * 12 + semitone;

            if (noteNumber < 0 || noteNumber > 127)
                return false;

            return true;
        }
    }
}
