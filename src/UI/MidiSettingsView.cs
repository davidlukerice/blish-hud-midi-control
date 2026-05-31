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
        private static readonly Blish_HUD.Logger Logger = Blish_HUD.Logger.GetLogger<MidiSettingsView>();

        private readonly MidiModule _module;

        private Dropdown? _deviceDropdown;
        private Label? _statusLabel;
        private Dropdown? _keymapDropdown;
        private Label? _logLabel;
        private Label? _previewLabel;
        private Panel? _previewPanel;
        private int _previewPanelBaseY;
        private int _previewPanelTallHeight;
        private int _previewPanelShortHeight;
        private Label? _keymapStatusLabel;
        private Action? _onLogUpdate;

        public MidiSettingsView(MidiModule module)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public void Build(Panel buildPanel)
        {
            buildPanel.ShowTint = true;

            int x = 20;
            int y = 15;

            // ---- MIDI Device Section ----
            var deviceHeader = new Label
            {
                Parent = buildPanel,
                Text = "MIDI Device",
                Location = new Point(x, y),
                TextColor = Color.FromNonPremultiplied(194, 181, 145, 255),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += deviceHeader.Height + 6;

            _deviceDropdown = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(x, y),
                Width = 220,
            };
            _deviceDropdown.ValueChanged += OnDeviceSelected;

            var refreshBtn = new StandardButton
            {
                Parent = buildPanel,
                Text = "Refresh",
                Location = new Point(248, y),
                Width = 80,
            };
            refreshBtn.Click += (s, e) => RefreshDevices();
            y += _deviceDropdown.Height + 6;

            _statusLabel = new Label
            {
                Parent = buildPanel,
                Text = _module.MidiDeviceStatus,
                Location = new Point(x, y),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                TextColor = Color.Gray,
            };
            _module.StatusLabel = _statusLabel;
            y += _statusLabel.Height + 10;

            // ---- Keymap Section ----
            var keymapHeader = new Label
            {
                Parent = buildPanel,
                Text = "Keymap",
                Location = new Point(x, y),
                TextColor = Color.FromNonPremultiplied(194, 181, 145, 255),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += keymapHeader.Height + 4;

            _keymapDropdown = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(x, y),
                Width = 220,
            };
            _keymapDropdown.ValueChanged += OnKeymapSelected;

            var reloadBtn = new StandardButton
            {
                Parent = buildPanel,
                Text = "Reload Keymaps",
                Location = new Point(248, y),
                Width = 110,
            };
            reloadBtn.Click += (s, e) => RefreshKeymaps();
            y += _keymapDropdown.Height + 4;

            _keymapStatusLabel = new Label
            {
                Parent = buildPanel,
                Text = "",
                Location = new Point(x, y),
                Height = 20,
                AutoSizeHeight = false,
                AutoSizeWidth = true,
                TextColor = Color.Gray,
            };
            RefreshKeymapStatusLabel();
            y += 24;

            _previewPanel = new Panel
            {
                Parent = buildPanel,
                Location = new Point(x, y),
                Size = new Point(420, 90),
                CanScroll = true,
            };
            _previewPanelBaseY = y;
            _previewPanelTallHeight = 114;   // 90 + 24 reclaimed from empty status row
            _previewPanelShortHeight = 90;

            _previewLabel = new Label
            {
                Parent = _previewPanel,
                Text = "",
                Location = new Point(0, 0),
                AutoSizeHeight = true,
                Width = 400,
                WrapText = true,
                TextColor = Color.LightGray,
            };
            y += _previewPanel.Height + 10;

            // ---- Standard Toggles ----
            var sendNotesCb = new Checkbox
            {
                Parent = buildPanel,
                Text = "Send Notes",
                Location = new Point(x, y),
                Checked = _module.SendNotesEnabled,
            };
            sendNotesCb.CheckedChanged += (s, e) => _module.SendNotesEnabled = e.Checked;
            y += sendNotesCb.Height + 4;

            var autoSwapCb = new Checkbox
            {
                Parent = buildPanel,
                Text = "Auto Swap Octave",
                Location = new Point(x, y),
                Checked = _module.AutoSwapOctaveEnabled,
            };
            autoSwapCb.CheckedChanged += (s, e) => _module.AutoSwapOctaveEnabled = e.Checked;
            y += autoSwapCb.Height + 4;

            var focusGuardCb = new Checkbox
            {
                Parent = buildPanel,
                Text = "Focus Guard",
                Location = new Point(x, y),
                Checked = _module.FocusGuardEnabled,
            };
            focusGuardCb.CheckedChanged += (s, e) => _module.FocusGuardEnabled = e.Checked;
            y += focusGuardCb.Height + 10;

            // ---- Delay Slider ----
            var delayLabel = new Label
            {
                Parent = buildPanel,
                Text = $"Multi-Octave Shift Delay: {_module.MultipleOctaveShiftDelay} ms",
                Location = new Point(x, y),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += delayLabel.Height + 4;

            var delaySlider = new TrackBar
            {
                Parent = buildPanel,
                Location = new Point(x, y),
                MinValue = 0,
                MaxValue = 500,
                Value = _module.MultipleOctaveShiftDelay,
                Width = 310,
            };
            delaySlider.ValueChanged += (s, e) =>
            {
                _module.MultipleOctaveShiftDelay = (int)delaySlider.Value;
                delayLabel.Text = $"Multi-Octave Shift Delay: {_module.MultipleOctaveShiftDelay} ms";
            };
            y += (int)delaySlider.Height + 10;

            // ---- Send Log (scrollable) ----
            var logHeader = new Label
            {
                Parent = buildPanel,
                Text = "Recent Sends",
                Location = new Point(x, y),
                TextColor = Color.FromNonPremultiplied(194, 181, 145, 255),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += logHeader.Height + 4;

            var logPanel = new Panel
            {
                Parent = buildPanel,
                Location = new Point(x, y),
                Size = new Point(420, 90),
                CanScroll = true,
            };

            _logLabel = new Label
            {
                Parent = logPanel,
                Text = _module.LastSendLog,
                Location = new Point(0, 0),
                AutoSizeHeight = true,
                Width = 400,
                WrapText = true,
                TextColor = Color.LightGray,
            };

            _onLogUpdate = () =>
            {
                if (_logLabel != null)
                    _logLabel.Text = _module.LastSendLog;
            };
            _module.RecentSendLogUpdated += _onLogUpdate;

            RefreshDevices();
            RefreshKeymaps();
        }

        private void RefreshDevices()
        {
            if (_deviceDropdown == null) return;

            _deviceDropdown.ValueChanged -= OnDeviceSelected;
            _deviceDropdown.Items.Clear();

            try
            {
                var devices = _module.AvailableMidiDevices;
                if (devices.Count == 0)
                {
                    Logger.Info("No MIDI devices detected by NAudio.");
                    _deviceDropdown.Items.Add("No MIDI devices found");
                    _deviceDropdown.Enabled = false;
                }
                else
                {
                    foreach (var device in devices)
                    {
                        _deviceDropdown.Items.Add(device);
                    }

                    _deviceDropdown.Enabled = true;

                    string? saved = _module.SelectedMidiDeviceName;
                    if (!string.IsNullOrEmpty(saved) && _deviceDropdown.Items.Contains(saved))
                        _deviceDropdown.SelectedItem = saved;
                    else if (_deviceDropdown.Items.Count > 0)
                        _deviceDropdown.SelectedItem = _deviceDropdown.Items[0];
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RefreshDevices failed.", ex);
                _deviceDropdown.Items.Add($"Error: {ex.Message}");
                _deviceDropdown.Enabled = false;
            }

            _deviceDropdown.ValueChanged += OnDeviceSelected;

            // Trigger device open for the initially-selected device
            // (set while the handler was detached above).
            if (_deviceDropdown.SelectedItem != null)
                OnDeviceSelected(null, EventArgs.Empty);
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

        private void RefreshKeymaps()
        {
            _module.ReloadKeymaps();
            PopulateKeymapDropdown();
            RefreshKeymapStatusLabel();
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
            RefreshPreview();
        }

        private void OnKeymapSelected(object? sender, EventArgs e)
        {
            var selectedName = _keymapDropdown?.SelectedItem;
            if (string.IsNullOrEmpty(selectedName)) return;

            var keymap = _module.AvailableKeymaps
                .FirstOrDefault(k => k.Name == selectedName);
            if (keymap != null)
            {
                _module.SelectKeymap(keymap.Id);
                RefreshPreview();
            }
        }

        public void Unload()
        {
            if (_onLogUpdate != null)
                _module.RecentSendLogUpdated -= _onLogUpdate;
        }

        private void RefreshKeymapStatusLabel()
        {
            if (_keymapStatusLabel == null || _previewPanel == null) return;

            int customCount = _module.CustomKeymapCount;
            var errors = _module.KeymapLoadErrors;
            int errorCount = errors.Count;

            if (customCount == 0 && errorCount == 0)
            {
                _keymapStatusLabel.Text = "";
                _keymapStatusLabel.BasicTooltipText = null;
                // Reclaim the status row space: raise and expand the preview panel.
                _previewPanel.Location = new Point(_previewPanel.Location.X, _previewPanelBaseY - (_previewPanelTallHeight - _previewPanelShortHeight));
                _previewPanel.Height = _previewPanelTallHeight;
                return;
            }

            string text = customCount switch
            {
                1 => $"1 custom keymap loaded",
                _ => $"{customCount} custom keymaps loaded",
            };

            if (errorCount > 0)
            {
                text += errorCount == 1 ? ", 1 error" : $", {errorCount} errors";
                _keymapStatusLabel.TextColor = Color.Orange;
            }
            else
            {
                _keymapStatusLabel.TextColor = Color.Gray;
            }

            _keymapStatusLabel.Text = text;

            if (errorCount > 0)
            {
                var tooltipLines = errors.Take(10).ToList();
                if (errors.Count > 10)
                    tooltipLines.Add($"(+{errors.Count - 10} more)");
                _keymapStatusLabel.BasicTooltipText = string.Join("\n", tooltipLines);
            }
            else
            {
                _keymapStatusLabel.BasicTooltipText = null;
            }

            // Status text is showing: preview panel at normal position and size.
            _previewPanel.Location = new Point(_previewPanel.Location.X, _previewPanelBaseY);
            _previewPanel.Height = _previewPanelShortHeight;
        }

        private void RefreshPreview()
        {
            if (_previewLabel == null) return;

            var selectedName = _keymapDropdown?.SelectedItem;
            if (string.IsNullOrEmpty(selectedName))
            {
                _previewLabel.Text = "";
                return;
            }

            var keymap = _module.AvailableKeymaps
                .FirstOrDefault(k => k.Name == selectedName);
            if (keymap == null)
            {
                _previewLabel.Text = "";
                return;
            }

            var lines = KeymapPreviewFormatter.FormatLines(keymap);
            _previewLabel.Text = string.Join("\n", lines);
        }
    }
}
