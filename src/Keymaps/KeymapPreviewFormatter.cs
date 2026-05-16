#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace DavidRice.BlishHud.MidiControl.Keymaps
{
    /// <summary>
    /// Formats a <see cref="Keymap"/> into human-readable preview lines for the settings UI.
    /// </summary>
    public static class KeymapPreviewFormatter
    {
        /// <summary>
        /// Returns one formatted line per mapped note, sorted by octave (from the note name),
        /// then note letter, then accidental (flat &lt; natural &lt; sharp).
        /// </summary>
        public static IReadOnlyList<string> FormatLines(Keymap keymap)
        {
            if (keymap == null)
                throw new ArgumentNullException(nameof(keymap));

            var lines = keymap.Notes
                .Select(kvp => FormatLine(kvp.Key, kvp.Value))
                .Where(line => line != null)
                .Cast<string>()
                .ToList();

            lines.Sort(CompareNoteLines);
            return lines;
        }

        private static string? FormatLine(string noteName, NoteDefinition definition)
        {
            if (definition.ForceInternalOctave.HasValue)
                return $"{noteName} → internal octave: {definition.ForceInternalOctave.Value}";

            if (definition.Key == null)
                return null; // Unmapped — skip.

            if (!definition.Octave.HasValue)
                return $"{noteName} → {definition.Key} (oct shift)";

            string line = $"{noteName} → {definition.Key} (oct {definition.Octave.Value})";

            if (definition.AltOctave.HasValue && definition.AltOctaveKey != null)
                line += $" | alt: {definition.AltOctaveKey} on oct {definition.AltOctave.Value}";

            return line;
        }

        private static int CompareNoteLines(string a, string b)
        {
            var keyA = ParseSortKey(ExtractNoteName(a));
            var keyB = ParseSortKey(ExtractNoteName(b));

            if (keyA.octave != keyB.octave)
                return keyA.octave.CompareTo(keyB.octave);

            if (keyA.letter != keyB.letter)
                return keyA.letter.CompareTo(keyB.letter);

            return keyA.accidental.CompareTo(keyB.accidental);
        }

        /// <summary>
        /// Extracts the note name from a formatted line (text before " → ").
        /// </summary>
        private static string ExtractNoteName(string line)
        {
            int arrow = line.IndexOf(" → ", StringComparison.Ordinal);
            return arrow > 0 ? line.Substring(0, arrow) : line;
        }

        /// <summary>
        /// Parses a note name like "C3", "C#4", or "Db5" into sortable components.
        /// </summary>
        private static (int octave, char letter, int accidental) ParseSortKey(string noteName)
        {
            int digitStart = 0;
            while (digitStart < noteName.Length && !char.IsDigit(noteName[digitStart]))
                digitStart++;

            string notePart = noteName.Substring(0, digitStart);
            int octave = digitStart < noteName.Length
                ? int.Parse(noteName.Substring(digitStart))
                : int.MaxValue;

            char letter = notePart[0];
            string accidentalStr = notePart.Length > 1 ? notePart.Substring(1) : "";

            int accidental = accidentalStr switch
            {
                "b" => 0,
                "" => 1,
                "#" => 2,
                _ => accidentalStr.StartsWith("b") ? 0
                     : accidentalStr.StartsWith("#") ? 2
                     : 1
            };

            return (octave, letter, accidental);
        }
    }
}
