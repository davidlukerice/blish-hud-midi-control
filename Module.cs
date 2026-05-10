#nullable enable

using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DavidRice.BlishHud.MidiControl.Input;
using DavidRice.BlishHud.MidiControl.Keymaps;

namespace DavidRice.BlishHud.MidiControl
{
    [Export(typeof(Module))]
    public class MidiModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<MidiModule>();

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        // ---- Settings ----
        private SettingEntry<string> _selectedMidiDeviceName = null!;
        private SettingEntry<string> _selectedKeymapId = null!;
        private SettingEntry<bool> _sendNotes = null!;
        private SettingEntry<bool> _autoSwapOctave = null!;
        private SettingEntry<int> _multipleOctaveShiftDelay = null!;
        private SettingEntry<bool> _focusGuard = null!;

        // ---- Subsystems ----
        private readonly ConcurrentQueue<Core.MidiNoteEvent> _midiQueue = new ConcurrentQueue<Core.MidiNoteEvent>();
        private Core.MidiInputManager _midiInputManager = null!;
        private KeymapRegistry _keymapRegistry = null!;
        private Core.KeySendThread _keySendThread = null!;

        // ---- UI ----
        private CornerIcon? _cornerIcon;
        private Texture2D? _activeIconTexture;
        private Texture2D? _mutedIconTexture;

        [ImportingConstructor]
        public MidiModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void DefineSettings(SettingCollection settings)
        {
            _selectedMidiDeviceName = settings.DefineSetting(
                "SelectedMidiDeviceName",
                string.Empty,
                "Selected MIDI Device",
                "Name of the MIDI input device used for playing.");

            _selectedKeymapId = settings.DefineSetting(
                "SelectedKeymapId",
                "minstrel-auto",
                "Selected Keymap",
                "The active keymap that maps MIDI notes to in-game keys.");

            _sendNotes = settings.DefineSetting(
                "SendNotes",
                true,
                "Send Notes",
                "If enabled, MIDI notes are sent as GW2 keyboard keypresses.");
            _sendNotes.SettingChanged += OnSendNotesChanged;

            _autoSwapOctave = settings.DefineSetting(
                "AutoSwapOctave",
                true,
                "Auto Swap Octave",
                "Automatically shift octaves when playing notes outside the current range.");

            _multipleOctaveShiftDelay = settings.DefineSetting(
                "MultipleOctaveShiftDelay",
                75,
                "Multi-Octave Shift Delay (ms)",
                "Delay between octave shift keypresses when shifting multiple octaves.");
            _focusGuard = settings.DefineSetting(
                "FocusGuard",
                true,
                "Focus Guard",
                "Block key sending when Guild Wars 2 is not in focus.");
        }

        protected override void Initialize()
        {
            _keymapRegistry = new KeymapRegistry();
            _midiInputManager = new Core.MidiInputManager(_midiQueue);
        }

        protected override async Task LoadAsync()
        {
            _keySendThread = new Core.KeySendThread();
            _keySendThread.Start();

            CreateCornerIcon();
            UpdateCornerIconState();

            // Attempt to re-open the previously selected device.
            if (!string.IsNullOrWhiteSpace(_selectedMidiDeviceName.Value))
            {
                bool opened = _midiInputManager.Open(_selectedMidiDeviceName.Value);
                if (!opened)
                {
                    Logger.Warn($"Could not re-open MIDI device '{_selectedMidiDeviceName.Value}'.");
                }
            }

            await Task.CompletedTask;
        }

        protected override void Update(GameTime gameTime)
        {
            // Drain the MIDI event queue.
            // KeySender is not wired yet — for now we only dequeue to keep the queue from growing unbounded.
            while (_midiQueue.TryDequeue(out Core.MidiNoteEvent noteEvent))
            {
#if DEBUG
                Logger.Debug($"MIDI event received: {noteEvent}");
#endif
            }
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);
        }

        protected override void Unload()
        {
            _sendNotes.SettingChanged -= OnSendNotesChanged;

            _cornerIcon?.Dispose();
            _cornerIcon = null;
            _activeIconTexture?.Dispose();
            _activeIconTexture = null;
            _mutedIconTexture?.Dispose();
            _mutedIconTexture = null;

            _keySendThread?.Dispose();
            _keySendThread = null!;

            _midiInputManager?.Dispose();
            _midiInputManager = null!;

            SafetyReleaseAllKeys();
        }

        // ---- Helpers ----

        private void CreateCornerIcon()
        {
            try
            {
                var gd = GameService.Graphics.GraphicsDevice;
                _activeIconTexture = CreateSolidTexture(gd, Color.Green, 16);
                _mutedIconTexture = CreateSolidTexture(gd, Color.Gray, 16);

                _cornerIcon = new CornerIcon
                {
                    Icon = _activeIconTexture,
                    BasicTooltipText = $"{Name} — Active",
                    Priority = 1645843523,
                    Parent = GameService.Graphics.SpriteScreen
                };

                _cornerIcon.Click += (s, e) =>
                {
                    // Toggle SendNotes when corner icon is clicked
                    _sendNotes.Value = !_sendNotes.Value;
                };
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to create corner icon: {ex.Message}");
            }
        }

        private void UpdateCornerIconState()
        {
            if (_cornerIcon == null) return;

            _cornerIcon.Icon = _sendNotes.Value ? _activeIconTexture : _mutedIconTexture;
            _cornerIcon.BasicTooltipText = $"{Name} — {(_sendNotes.Value ? "Active" : "Muted")}";
        }

        private void OnSendNotesChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            UpdateCornerIconState();
        }

        private static Texture2D CreateSolidTexture(GraphicsDevice device, Color color, int size)
        {
            var texture = new Texture2D(device, size, size);
            var data = new Color[size * size];
            for (int i = 0; i < data.Length; i++)
                data[i] = color;
            texture.SetData(data);
            return texture;
        }

        private static void SafetyReleaseAllKeys()
        {
            // Best-effort key-up for all possible GW2 instrument and octave-shift keys.
            // This prevents stuck keys if Blish HUD closes while a key is held.
            var scanCodes = new uint[]
            {
                0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, // 1–8
                0x0A, 0x0B // 9, 0
            };

            foreach (var sc in scanCodes)
            {
                try
                {
                    SendInputApi.SendKeyUp(sc);
                }
                catch
                {
                    // Ignore on unload
                }
            }
        }
    }
}
