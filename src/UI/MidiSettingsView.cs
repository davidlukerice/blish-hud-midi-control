#nullable enable

using System;
using System.Linq;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps;

namespace DavidRice.BlishHud.MidiControl.UI
{
    /// <summary>
    /// Custom settings panel for the MIDI Control module. Shows a MIDI device dropdown with
    /// refresh, keymap selection, standard toggles, a delay slider, and a recent-send log.
    /// </summary>
    public class MidiSettingsView
    {
        private readonly MidiModule _module;

        private Dropdown? _deviceDropdown;
        private Label? _statusLabel;
        private Dropdown? _keymapDropdown;
        private Label? _logLabel;

        public MidiSettingsView(MidiModule module)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public void Build(Panel buildPanel)
        {
            buildPanel.CanScroll = true;
            buildPanel.ShowTint = true;

            int y = 10;

            // ---- MIDI Device Section ----
            var deviceHeader = new Label
            {
                Parent = buildPanel,
                Text = "MIDI Device",
                Location = new Point(10, y),
                TextColor = Color.FromNonPremultiplied(194, 181, 145, 255),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += deviceHeader.Height + 6;

            _deviceDropdown = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(10, y),
                Width = 270,
            };
            _deviceDropdown.ValueChanged += OnDeviceSelected;

            var refreshBtn = new StandardButton
            {
                Parent = buildPanel,
                Text = "Refresh",
                Location = new Point(290, y),
                Width = 80,
            };
            refreshBtn.Click += (s, e) => RefreshDevices();
            y += _deviceDropdown.Height + 6;

            _statusLabel = new Label
            {
                Parent = buildPanel,
                Text = _module.MidiDeviceStatus,
                Location = new Point(10, y),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                TextColor = Color.Gray,
            };
            y += _statusLabel.Height + 20;

            // ---- Keymap Section ----
            var keymapHeader = new Label
            {
                Parent = buildPanel,
                Text = "Keymap",
                Location = new Point(10, y),
                TextColor = Color.FromNonPremultiplied(194, 181, 145, 255),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += keymapHeader.Height + 6;

            _keymapDropdown = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(10, y),
                Width = 270,
            };
            _keymapDropdown.ValueChanged += OnKeymapSelected;
            y += _keymapDropdown.Height + 20;

            // ---- Standard Toggles ----
            var sendNotesCb = new Checkbox
            {
                Parent = buildPanel,
                Text = "Send Notes",
                Location = new Point(10, y),
                Checked = _module.SendNotesEnabled,
            };
            sendNotesCb.CheckedChanged += (s, e) => _module.SendNotesEnabled = e.Checked;
            y += sendNotesCb.Height + 6;

            var autoSwapCb = new Checkbox
            {
                Parent = buildPanel,
                Text = "Auto Swap Octave",
                Location = new Point(10, y),
                Checked = _module.AutoSwapOctaveEnabled,
            };
            autoSwapCb.CheckedChanged += (s, e) => _module.AutoSwapOctaveEnabled = e.Checked;
            y += autoSwapCb.Height + 6;

            var focusGuardCb = new Checkbox
            {
                Parent = buildPanel,
                Text = "Focus Guard",
                Location = new Point(10, y),
                Checked = _module.FocusGuardEnabled,
            };
            focusGuardCb.CheckedChanged += (s, e) => _module.FocusGuardEnabled = e.Checked;
            y += focusGuardCb.Height + 16;

            // ---- Delay Slider ----
            var delayLabel = new Label
            {
                Parent = buildPanel,
                Text = $"Multi-Octave Shift Delay: {_module.MultipleOctaveShiftDelay} ms",
                Location = new Point(10, y),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += delayLabel.Height + 6;

            var delaySlider = new TrackBar
            {
                Parent = buildPanel,
                Location = new Point(10, y),
                MinValue = 0,
                MaxValue = 500,
                Value = _module.MultipleOctaveShiftDelay,
                Width = 300,
            };
            delaySlider.ValueChanged += (s, e) =>
            {
                _module.MultipleOctaveShiftDelay = (int)delaySlider.Value;
                delayLabel.Text = $"Multi-Octave Shift Delay: {_module.MultipleOctaveShiftDelay} ms";
            };
            y += (int)delaySlider.Height + 20;

            // ---- Send Log ----
            var logHeader = new Label
            {
                Parent = buildPanel,
                Text = "Recent Sends",
                Location = new Point(10, y),
                TextColor = Color.FromNonPremultiplied(194, 181, 145, 255),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += logHeader.Height + 6;

            _logLabel = new Label
            {
                Parent = buildPanel,
                Text = _module.LastSendLog,
                Location = new Point(10, y),
                AutoSizeHeight = true,
                Width = 400,
                WrapText = true,
                TextColor = Color.LightGray,
            };

            RefreshDevices();
            PopulateKeymapDropdown();
        }

        private void RefreshDevices()
        {
            if (_deviceDropdown == null) return;
            _deviceDropdown.ValueChanged -= OnDeviceSelected;
            _deviceDropdown.Items.Clear();

            var devices = _module.AvailableMidiDevices;
            if (devices.Count == 0)
            {
                _deviceDropdown.Items.Add("No MIDI devices found");
                _deviceDropdown.Enabled = false;
            }
            else
            {
                foreach (var device in devices)
                    _deviceDropdown.Items.Add(device);
                _deviceDropdown.Enabled = true;

                string? saved = _module.SelectedMidiDeviceName;
                if (!string.IsNullOrEmpty(saved) && _deviceDropdown.Items.Contains(saved))
                    _deviceDropdown.SelectedItem = saved;
                else if (_deviceDropdown.Items.Count > 0)
                    _deviceDropdown.SelectedItem = _deviceDropdown.Items[0];
            }

            _deviceDropdown.ValueChanged += OnDeviceSelected;
        }

        private void OnDeviceSelected(object? sender, EventArgs e)
        {
            var selected = _deviceDropdown?.SelectedItem;
            if (string.IsNullOrEmpty(selected) || selected == "No MIDI devices found")
                return;

            if (_module.SelectedMidiDeviceName == selected)
                return; // Already the active device.

            _module.OpenMidiDevice(selected!);
            if (_statusLabel != null)
                _statusLabel.Text = _module.MidiDeviceStatus;
        }

        private void PopulateKeymapDropdown()
        {
            if (_keymapDropdown == null) return;
            _keymapDropdown.ValueChanged -= OnKeymapSelected;
            _keymapDropdown.Items.Clear();

            foreach (var keymap in _module.AvailableKeymaps)
                _keymapDropdown.Items.Add(keymap.Name);

            var current = _module.AvailableKeymaps
                .FirstOrDefault(k => k.Id == _module.SelectedKeymapId);
            if (current != null)
                _keymapDropdown.SelectedItem = current.Name;
            else if (_keymapDropdown.Items.Count > 0)
                _keymapDropdown.SelectedItem = _keymapDropdown.Items[0];

            _keymapDropdown.ValueChanged += OnKeymapSelected;
        }

        private void OnKeymapSelected(object? sender, EventArgs e)
        {
            var selectedName = _keymapDropdown?.SelectedItem;
            if (string.IsNullOrEmpty(selectedName)) return;

            var keymap = _module.AvailableKeymaps
                .FirstOrDefault(k => k.Name == selectedName);
            if (keymap != null)
                _module.SelectKeymap(keymap.Id);
        }
    }
}
