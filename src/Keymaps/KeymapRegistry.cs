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

        private readonly List<Keymap> _keymaps = new List<Keymap>();
        private readonly List<string> _loadErrors = new List<string>();

        public KeymapRegistry()
        {
            Register(GeneralKeymap.Instance);
            Register(GrandPianoAutoKeymap.Instance);
            Register(BassGuitarAutoKeymap.Instance);
            Register(FluteCAutoKeymap.Instance);
            Register(FluteEAutoKeymap.Instance);
            Register(HarpAutoKeymap.Instance);
            Register(HornCAutoKeymap.Instance);
            Register(HornEAutoKeymap.Instance);
            Register(LuteAutoKeymap.Instance);
            Register(ChoirBellAutoKeymap.Instance);
            Register(MinstrelKeymap.Instance);
            Register(MinstrelAutoKeymap.Instance);
            Register(VerdarachAutoKeymap.Instance);
        }

        public IReadOnlyList<Keymap> AllKeymaps => _keymaps.AsReadOnly();

        public IReadOnlyList<string> LoadErrors => _loadErrors.AsReadOnly();

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

        public void LoadCustomKeymaps(string directoryPath)
        {
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

                    Register(keymap);
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
