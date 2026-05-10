#nullable enable

using System.Collections.Generic;

namespace DavidRice.BlishHud.MidiControl
{
    public class NoteDefinition
    {
        public string? Key { get; }
        public int? Octave { get; }
        public int? AltOctave { get; }
        public string? AltOctaveKey { get; }
        public int? ForceInternalOctave { get; }

        public NoteDefinition(
            string? key = null,
            int? octave = null,
            int? altOctave = null,
            string? altOctaveKey = null,
            int? forceInternalOctave = null)
        {
            Key = key;
            Octave = octave;
            AltOctave = altOctave;
            AltOctaveKey = altOctaveKey;
            ForceInternalOctave = forceInternalOctave;
        }
    }

    public class Keymap
    {
        public string Id { get; }
        public string Name { get; }
        public bool AutoOctaveSwap { get; set; } = true;
        public Dictionary<string, NoteDefinition> Notes { get; } = new Dictionary<string, NoteDefinition>();
        public string? OctaveDownKey { get; set; }
        public string? OctaveUpKey { get; set; }

        public Keymap(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
