#nullable enable

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
    }
}
