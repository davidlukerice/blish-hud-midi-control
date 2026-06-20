#nullable enable

namespace DavidRice.BlishHud.MidiControl.Keymaps.Visualization
{
    public class KeybedKey
    {
        public int NoteNumber { get; }
        public string NoteName { get; }
        public bool IsBlackKey { get; }
        public bool IsMapped { get; }
        public string? Gw2Key { get; }
        public bool IsKeySwitch { get; }
        public int? Octave { get; }
        public bool HasAltOctave { get; }
        public int? AltOctave { get; }
        public string? AltOctaveKey { get; }

        public KeybedKey(
            int noteNumber,
            string noteName,
            bool isBlackKey,
            bool isMapped,
            string? gw2Key,
            bool isKeySwitch,
            int? octave,
            bool hasAltOctave,
            int? altOctave,
            string? altOctaveKey)
        {
            NoteNumber = noteNumber;
            NoteName = noteName;
            IsBlackKey = isBlackKey;
            IsMapped = isMapped;
            Gw2Key = gw2Key;
            IsKeySwitch = isKeySwitch;
            Octave = octave;
            HasAltOctave = hasAltOctave;
            AltOctave = altOctave;
            AltOctaveKey = altOctaveKey;
        }
    }
}
