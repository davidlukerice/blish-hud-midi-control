#nullable enable

using System.Collections.Generic;
using System.Linq;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Keymaps
{
    public class KeymapRegistry
    {
        private readonly List<Keymap> _keymaps = new List<Keymap>();

        public KeymapRegistry()
        {
            Register(GrandPianoAutoKeymap.Instance);
            Register(MinstrelAutoKeymap.Instance);
            Register(MinstrelKeymap.Instance);
            Register(ChoirBellAutoKeymap.Instance);
            Register(FluteCAutoKeymap.Instance);
            Register(FluteEAutoKeymap.Instance);
        }

        public IReadOnlyList<Keymap> AllKeymaps => _keymaps.AsReadOnly();

        public void Register(Keymap keymap)
        {
            _keymaps.Add(keymap);
        }

        public Keymap? FindById(string id)
        {
            return _keymaps.FirstOrDefault(k => k.Id == id);
        }

        public Keymap? FindByName(string name)
        {
            return _keymaps.FirstOrDefault(k => k.Name == name);
        }
    }
}
