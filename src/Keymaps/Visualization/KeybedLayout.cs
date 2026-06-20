#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DavidRice.BlishHud.MidiControl.Keymaps.Visualization
{
    public class KeybedLayout
    {
        public IReadOnlyList<KeybedKey> Keys { get; }
        public int StartOctave { get; }
        public int EndOctave { get; }
        public bool IsEmpty => Keys.Count == 0;

        public KeybedLayout(IEnumerable<KeybedKey> keys, int startOctave, int endOctave)
        {
            Keys = new ReadOnlyCollection<KeybedKey>(new List<KeybedKey>(keys));
            StartOctave = startOctave;
            EndOctave = endOctave;
        }

        public static KeybedLayout Empty { get; } = new KeybedLayout(new List<KeybedKey>(), 0, 0);
    }
}
