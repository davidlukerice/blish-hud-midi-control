#nullable enable

using System.Collections.Generic;

namespace DavidRice.BlishHud.MidiControl
{
    public class NoteDefinition
    {
        public string? Key { get; set; }
        public int? Octave { get; set; }
        public int? AltOctave { get; set; }
        public string? AltOctaveKey { get; set; }
        public int? ForceInternalOctave { get; set; }

        public NoteDefinition()
        {
        }

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
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool AutoOctaveSwap { get; set; } = true;
        public Dictionary<string, NoteDefinition> Notes { get; set; } = new Dictionary<string, NoteDefinition>();
        public string? OctaveDownKey { get; set; }
        public string? OctaveUpKey { get; set; }

        public Keymap()
        {
        }

        public Keymap(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
