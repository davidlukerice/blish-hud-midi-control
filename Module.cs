#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DavidRice.BlishHud.MidiControl.Input;
using DavidRice.BlishHud.MidiControl.Keymaps;
using DavidRice.BlishHud.MidiControl.UI;

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
        private SettingCollection _settingsCollection = null!;
        private SettingEntry<string> _selectedMidiDeviceName = null!;
        private SettingEntry<string> _selectedKeymapId = null!;
        private SettingEntry<bool> _sendNotes = null!;
        private SettingEntry<bool> _autoSwapOctave = null!;
        private SettingEntry<int> _multipleOctaveShiftDelay = null!;
        private SettingEntry<bool> _focusGuard = null!;
        private SettingEntry<KeyBinding> _toggleSendNotesKeybind = null!;

        // ---- Subsystems ----
        private readonly ConcurrentQueue<Core.MidiNoteEvent> _midiQueue = new ConcurrentQueue<Core.MidiNoteEvent>();
        private Core.MidiInputManager _midiInputManager = null!;
        private KeymapRegistry _keymapRegistry = null!;
        private Core.KeySendThread _keySendThread = null!;
        private Core.KeySender _keySender = null!;

        // ---- Diagnostics ----
        private readonly Queue<string> _recentSendLog = new Queue<string>(10);
        public event Action? RecentSendLogUpdated;

        // ---- UI ----
        private CornerIcon? _cornerIcon;
        private Texture2D? _activeIconTexture;
        private Texture2D? _mutedIconTexture;
        private Texture2D? _disconnectedIconTexture;
        private TabbedWindow2? _settingsWindow;
        private Blish_HUD.Content.AsyncTexture2D? _settingsTabIcon;

        [ImportingConstructor]
        public MidiModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void DefineSettings(SettingCollection settings)
        {
            _settingsCollection = settings;

            _selectedMidiDeviceName = settings.DefineSetting(
                "SelectedMidiDeviceName",
                string.Empty,
                () => "Selected MIDI Device",
                () => "Name of the MIDI input device used for playing.");

            _selectedKeymapId = settings.DefineSetting(
                "SelectedKeymapId",
                "minstrel-auto",
                () => "Selected Keymap",
                () => "The active keymap that maps MIDI notes to in-game keys.");

            _sendNotes = settings.DefineSetting(
                "SendNotes",
                true,
                () => "Send Notes",
                () => "If enabled, MIDI notes are sent as GW2 keyboard keypresses.");
            _sendNotes.SettingChanged += OnSendNotesChanged;
            _selectedKeymapId.SettingChanged += OnKeymapChanged;

            _autoSwapOctave = settings.DefineSetting(
                "AutoSwapOctave",
                true,
                () => "Auto Swap Octave",
                () => "Automatically shift octaves when playing notes outside the current range.");

            _multipleOctaveShiftDelay = settings.DefineSetting(
                "MultipleOctaveShiftDelay",
                75,
                () => "Multi-Octave Shift Delay (ms)",
                () => "Delay between octave shift keypresses when shifting multiple octaves.");
            _focusGuard = settings.DefineSetting(
                "FocusGuard",
                true,
                () => "Focus Guard",
                () => "Block key sending when Guild Wars 2 is not in focus.");

            _toggleSendNotesKeybind = settings.DefineSetting(
                "ToggleSendNotesKeybind",
                new KeyBinding(),
                () => "Toggle Send Notes",
                () => "Keybind to toggle Send Notes on or off.");
        }

        protected override void Initialize()
        {
            _keymapRegistry = new KeymapRegistry();
            _midiInputManager = new Core.MidiInputManager(_midiQueue);

            _recentSendLog.Enqueue("No sends yet.");
        }

        protected override async Task LoadAsync()
        {
            _keySendThread = new Core.KeySendThread();
            _keySendThread.Start();

            _keySender = new Core.KeySender(_keySendThread);
            _keySender.NoteProcessed += OnNoteProcessed;

            _toggleSendNotesKeybind.Value.Enabled = true;
            _toggleSendNotesKeybind.Value.BlockSequenceFromGw2 = true;
            _toggleSendNotesKeybind.Value.IgnoreWhenInTextField = false;
            _toggleSendNotesKeybind.Value.Activated += OnToggleSendNotesKeybind;

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

        private bool _wasRetrying;

        protected override void Update(GameTime gameTime)
        {
            bool isRetrying = _midiInputManager.IsRetryingConnection;

            _midiInputManager.CheckConnection(_selectedMidiDeviceName.Value);

            // Refresh corner icon if retrying state changed (disconnect/reconnect).
            if (isRetrying != _wasRetrying)
            {
                _wasRetrying = isRetrying;
                UpdateCornerIconState();
            }

            // Push live status to the settings panel if it's open.
            if (StatusLabel != null)
                StatusLabel.Text = MidiDeviceStatus;

            while (_midiQueue.TryDequeue(out Core.MidiNoteEvent noteEvent))
            {
                if (!_sendNotes.Value)
                    continue;

                if (_focusGuard.Value && !GameService.GameIntegration.Gw2Instance.Gw2IsRunning)
                    continue;

                Keymap? keymap = GetActiveKeymap();
                if (keymap == null)
                    continue;

                _keySender.Send(
                    noteEvent,
                    keymap,
                    _autoSwapOctave.Value,
                    _multipleOctaveShiftDelay.Value);
            }
        }

        protected override void Unload()
        {
            _sendNotes.SettingChanged -= OnSendNotesChanged;
            _selectedKeymapId.SettingChanged -= OnKeymapChanged;
            _keySender.NoteProcessed -= OnNoteProcessed;
            _toggleSendNotesKeybind.Value.Enabled = false;
            _toggleSendNotesKeybind.Value.Activated -= OnToggleSendNotesKeybind;

            _settingsWindow?.Dispose();
            _settingsWindow = null;
            StatusLabel = null;

            _cornerIcon?.Dispose();
            _cornerIcon = null;
            _activeIconTexture?.Dispose();
            _activeIconTexture = null;
            _mutedIconTexture?.Dispose();
            _mutedIconTexture = null;
            _disconnectedIconTexture?.Dispose();
            _disconnectedIconTexture = null;

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
                _activeIconTexture = ContentsManager.GetTexture("icon.png");
                _mutedIconTexture = ContentsManager.GetTexture("icon_off.png");
                _disconnectedIconTexture = ContentsManager.GetTexture("icon_disconnected.png");
                _settingsTabIcon = new Blish_HUD.Content.AsyncTexture2D(ContentsManager.GetTexture("icon.png"));

                _cornerIcon = new CornerIcon
                {
                    Icon = _activeIconTexture,
                    BasicTooltipText = $"{Name} — Active",
                    Priority = 1645843523,
                    Parent = GameService.Graphics.SpriteScreen
                };

                _cornerIcon.Click += (s, e) =>
                {
                    try
                    {
                        if (_settingsWindow == null)
                        {
                            var bg = Blish_HUD.Content.AsyncTexture2D.FromAssetId(155997);
                            _settingsWindow = new TabbedWindow2(
                                bg,
                                new Rectangle(24, 30, 545, 630),
                                new Rectangle(82, 30, 467, 600))
                            {
                                Parent = GameService.Graphics.SpriteScreen,
                                Title = "MIDI Control",
                                SavesPosition = true,
                                Id = $"{nameof(MidiModule)}_Settings_6a2b3c4d"
                            };
                            _settingsWindow.Tabs.Add(new Tab(_settingsTabIcon, () => new MidiSettingsTabView(this), "Settings"));
                        }
                        _settingsWindow.ToggleWindow();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Corner icon click failed.", ex);
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to create corner icon.", ex);
            }
        }

        private void UpdateCornerIconState()
        {
            if (_cornerIcon == null) return;

            if (_midiInputManager?.IsRetryingConnection == true)
            {
                _cornerIcon.Icon = _disconnectedIconTexture;
                _cornerIcon.BasicTooltipText = $"{Name} — Disconnected";
                return;
            }

            _cornerIcon.Icon = _sendNotes.Value ? _activeIconTexture : _mutedIconTexture;
            _cornerIcon.BasicTooltipText = $"{Name} — {(_sendNotes.Value ? "Active" : "Muted")}";
        }

        private void OnSendNotesChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            UpdateCornerIconState();
        }

        private void OnToggleSendNotesKeybind(object sender, EventArgs e)
        {
            _sendNotes.Value = !_sendNotes.Value;
        }

        private void OnKeymapChanged(object sender, ValueChangedEventArgs<string> e)
        {
            // Fresh KeySender so the internal octave tracker resets to 0.
            if (_keySendThread != null)
            {
                _keySender.NoteProcessed -= OnNoteProcessed;
                _keySender = new Core.KeySender(_keySendThread);
                _keySender.NoteProcessed += OnNoteProcessed;
            }
        }

        private void OnNoteProcessed(Core.MidiNoteEvent noteEvent, Core.KeySendResult result)
        {
            if (result.SentKeyNames.Length == 0)
                return;

            string noteName = Core.MidiNote.GetNoteName(noteEvent.NoteNumber);
            string keys = string.Join(" + ", result.SentKeyNames);
            string octavePrefix = result.PreviousOctave == result.NewOctave
                ? $"oct {result.NewOctave}"
                : $"oct {result.PreviousOctave}→{result.NewOctave}";
            string desc = $"{octavePrefix}: {noteName} → {keys}";

            if (_recentSendLog.Count > 0 && _recentSendLog.Peek() == "No sends yet.")
                _recentSendLog.Dequeue();

            _recentSendLog.Enqueue(desc);
            while (_recentSendLog.Count > 10)
                _recentSendLog.Dequeue();

            RecentSendLogUpdated?.Invoke();
        }

        private Keymap? GetActiveKeymap()
        {
            string id = _selectedKeymapId.Value;
            Keymap? keymap = _keymapRegistry.FindById(id);
            if (keymap != null)
                return keymap;

            Logger.Warn($"Keymap '{id}' not found; falling back to 'minstrel-auto'.");
            keymap = _keymapRegistry.FindById("minstrel-auto");
            if (keymap != null)
                return keymap;

            Logger.Error("Fallback keymap 'minstrel-auto' not found. No notes will be sent.");
            return null;
        }

        internal Label? StatusLabel { get; set; }

        public IReadOnlyList<string> AvailableMidiDevices => Core.MidiInputManager.AvailableDevices;
        public IReadOnlyList<Keymap> AvailableKeymaps => _keymapRegistry.AllKeymaps;

        public string SelectedMidiDeviceName => _selectedMidiDeviceName.Value;
        public string SelectedKeymapId => _selectedKeymapId.Value;

        public bool SendNotesEnabled
        {
            get => _sendNotes.Value;
            set => _sendNotes.Value = value;
        }

        public bool AutoSwapOctaveEnabled
        {
            get => _autoSwapOctave.Value;
            set => _autoSwapOctave.Value = value;
        }

        public bool FocusGuardEnabled
        {
            get => _focusGuard.Value;
            set => _focusGuard.Value = value;
        }

        public int MultipleOctaveShiftDelay
        {
            get => _multipleOctaveShiftDelay.Value;
            set => _multipleOctaveShiftDelay.Value = value;
        }

        public string MidiDeviceStatus
        {
            get
            {
                if (_midiInputManager?.IsRetryingConnection == true)
                    return $"Disconnected — retrying (was: {_selectedMidiDeviceName.Value})";
                if (_midiInputManager?.IsDeviceOpen == true && !string.IsNullOrEmpty(_midiInputManager.ActiveDeviceName))
                    return $"Connected: {_midiInputManager.ActiveDeviceName}";
                if (!string.IsNullOrEmpty(_selectedMidiDeviceName.Value))
                    return $"Not connected. Saved: {_selectedMidiDeviceName.Value}";
                return "No device selected.";
            }
        }

        public string LastSendLog
        {
            get
            {
                if (_recentSendLog.Count == 0)
                    return "No sends yet.";
                return string.Join("\n", _recentSendLog.Reverse());
            }
        }

        public void OpenMidiDevice(string deviceName)
        {
            bool opened = _midiInputManager.Open(deviceName);
            if (opened)
            {
                _selectedMidiDeviceName.Value = deviceName;
                Logger.Info($"MIDI device opened: {deviceName}");
            }
            else
            {
                Logger.Warn($"Failed to open MIDI device: {deviceName}");
            }
        }

        public void SelectKeymap(string id)
        {
            _selectedKeymapId.Value = id;
        }

        // ---- Helpers ----

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

        private sealed class MidiSettingsTabView : IView
        {
            private static readonly Logger Logger = Logger.GetLogger<MidiSettingsTabView>();

            private readonly MidiModule _module;
            private MidiSettingsView? _view;

            public MidiSettingsTabView(MidiModule module)
            {
                _module = module;
            }

            public bool WithPresenter => false;

#pragma warning disable CS0067
            public event EventHandler<EventArgs>? Built;
            public event EventHandler<EventArgs>? Loaded;
            public event EventHandler<EventArgs>? Unloaded;
#pragma warning restore CS0067

            public Task<bool> DoLoad(IProgress<string> progress)
            {
                progress.Report("MIDI Control settings loaded.");
                return Task.FromResult(true);
            }

            public void DoBuild(Container buildPanel)
            {
                var panel = new Panel
                {
                    Parent = buildPanel,
                    Size = buildPanel.Size,
                };

                try
                {
                    _view = new MidiSettingsView(_module);
                    _view.Build(panel);
                }
                catch (Exception ex)
                {
                    Logger.Error("Build threw.", ex);
                }

            }

            public void DoUnload()
            {
                _view?.Unload();
            }
        }
    }
}
