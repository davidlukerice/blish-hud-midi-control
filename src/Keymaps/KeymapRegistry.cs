#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blish_HUD;
using Newtonsoft.Json;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Keymaps
{
    public class KeymapRegistry
    {
        private static readonly Logger Logger = Logger.GetLogger<KeymapRegistry>();

        private readonly List<Keymap> _builtInKeymaps = new List<Keymap>();
        private readonly List<Keymap> _customKeymaps = new List<Keymap>();
        private readonly List<string> _loadErrors = new List<string>();

        public KeymapRegistry()
        {
            RegisterBuiltIn(GeneralKeymap.Instance);
            RegisterBuiltIn(GrandPianoAutoKeymap.Instance);
            RegisterBuiltIn(BassGuitarAutoKeymap.Instance);
            RegisterBuiltIn(FluteCAutoKeymap.Instance);
            RegisterBuiltIn(FluteEAutoKeymap.Instance);
            RegisterBuiltIn(HarpAutoKeymap.Instance);
            RegisterBuiltIn(HornCAutoKeymap.Instance);
            RegisterBuiltIn(HornEAutoKeymap.Instance);
            RegisterBuiltIn(LuteAutoKeymap.Instance);
            RegisterBuiltIn(ChoirBellAutoKeymap.Instance);
            RegisterBuiltIn(MinstrelKeymap.Instance);
            RegisterBuiltIn(MinstrelAutoKeymap.Instance);
            RegisterBuiltIn(VerdarachAutoKeymap.Instance);
            RegisterBuiltIn(FrameDrumAutoKeymap.Instance);
            RegisterBuiltIn(BlackLionDrumKeymap.Instance);
        }

        public IReadOnlyList<Keymap> AllKeymaps =>
            _builtInKeymaps.Concat(_customKeymaps).ToList().AsReadOnly();

        public int CustomKeymapCount => _customKeymaps.Count;

        public IReadOnlyList<string> LoadErrors => _loadErrors.AsReadOnly();

        public void Register(Keymap keymap)
        {
            _builtInKeymaps.Add(keymap);
        }

        private void RegisterBuiltIn(Keymap keymap)
        {
            _builtInKeymaps.Add(keymap);
        }

        public Keymap? FindById(string id)
        {
            return _builtInKeymaps.FirstOrDefault(k => k.Id == id)
                ?? _customKeymaps.FirstOrDefault(k => k.Id == id);
        }

        public Keymap? FindByName(string name)
        {
            return _builtInKeymaps.FirstOrDefault(k => k.Name == name)
                ?? _customKeymaps.FirstOrDefault(k => k.Name == name);
        }

        public void LoadCustomKeymaps(string directoryPath)
        {
            _customKeymaps.Clear();
            _loadErrors.Clear();

            if (!Directory.Exists(directoryPath))
            {
                Logger.Info($"Custom keymaps directory not found: {directoryPath}");
                return;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(directoryPath, "*.json");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to scan custom keymaps directory: {ex.Message}");
                _loadErrors.Add($"Failed to scan directory: {ex.Message}");
                return;
            }

            int loadedCount = 0;

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);

                try
                {
                    string fileText = File.ReadAllText(filePath);
                    Keymap? keymap = JsonConvert.DeserializeObject<Keymap>(fileText);

                    if (keymap == null)
                    {
                        _loadErrors.Add($"{fileName}: deserialization returned null");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(keymap.Id))
                    {
                        _loadErrors.Add($"{fileName}: missing required field 'id'");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(keymap.Name))
                    {
                        _loadErrors.Add($"{fileName}: missing required field 'name'");
                        continue;
                    }

                    if (keymap.Notes == null)
                    {
                        _loadErrors.Add($"{fileName}: missing required field 'notes'");
                        continue;
                    }

                    if (FindById(keymap.Id) != null)
                    {
                        _loadErrors.Add($"{fileName}: id '{keymap.Id}' conflicts with existing keymap");
                        continue;
                    }

                    _customKeymaps.Add(keymap);
                    loadedCount++;
                }
                catch (JsonException ex)
                {
                    _loadErrors.Add($"{fileName}: JSON parse error — {ex.Message}");
                }
                catch (Exception ex)
                {
                    _loadErrors.Add($"{fileName}: read error — {ex.Message}");
                }
            }

            Logger.Info($"Loaded {loadedCount} custom keymap(s) from {directoryPath}.");
            if (_loadErrors.Count > 0)
            {
                Logger.Warn($"{_loadErrors.Count} custom keymap file(s) had errors:");
                foreach (string error in _loadErrors)
                {
                    Logger.Warn($"  {error}");
                }
            }
        }
    }
}
