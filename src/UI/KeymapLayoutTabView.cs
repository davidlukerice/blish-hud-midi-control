#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps;
using DavidRice.BlishHud.MidiControl.Keymaps.Visualization;

namespace DavidRice.BlishHud.MidiControl.UI
{
    /// <summary>
    /// Settings tab that shows a visual keybed preview for any selected keymap.
    /// Selecting a keymap here also updates the active playing keymap.
    /// </summary>
    public class KeymapLayoutTabView : IView
    {
        private static readonly Logger Logger = Logger.GetLogger<KeymapLayoutTabView>();

        private readonly MidiModule _module;
        private KeybedControl? _keybedControl;
        private Panel? _keybedPanel;
        private TrackBar? _scrollTrackBar;
        private Dropdown? _keymapDropdown;
        private Label? _keymapInfoLabel;

        public KeymapLayoutTabView(MidiModule module)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public bool WithPresenter => false;

#pragma warning disable CS0067
        public event EventHandler<EventArgs>? Built;
        public event EventHandler<EventArgs>? Loaded;
        public event EventHandler<EventArgs>? Unloaded;
#pragma warning restore CS0067

        public Task<bool> DoLoad(IProgress<string> progress)
        {
            progress.Report("Keymap Layout loaded.");
            return Task.FromResult(true);
        }

        public void DoBuild(Container buildPanel)
        {
            int x = 20;
            int y = 15;

            // ---- Keymap selector ----
            var selectLabel = new Label
            {
                Parent = buildPanel,
                Text = "Keymap",
                Location = new Point(x, y),
                TextColor = Color.FromNonPremultiplied(194, 181, 145, 255),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            y += selectLabel.Height + 6;

            _keymapDropdown = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(x, y),
                Width = 260,
            };
            _keymapDropdown.ValueChanged += OnKeymapSelected;
            y += _keymapDropdown.Height + 8;

            // ---- Keymap info label ----
            _keymapInfoLabel = new Label
            {
                Parent = buildPanel,
                Text = "",
                Location = new Point(x, y),
                Height = 20,
                AutoSizeHeight = false,
                AutoSizeWidth = true,
                TextColor = Color.Gray,
            };
            y += 28;

            // ---- Keybed preview ----
            _keybedPanel = new Panel
            {
                Parent = buildPanel,
                Location = new Point(x, y),
                Size = new Point(420, 104),
            };

            _keybedControl = new KeybedControl
            {
                Parent = _keybedPanel,
                Location = new Point(0, 0),
                Layout = KeybedLayout.Empty,
            };

            _scrollTrackBar = new TrackBar
            {
                Parent = buildPanel,
                Location = new Point(x, y + _keybedPanel.Height + 4),
                Size = new Point(420, 20),
                MinValue = 0,
                MaxValue = 0,
                Value = 0,
            };
            _scrollTrackBar.ValueChanged += OnScrollValueChanged;

            PopulateDropdown();
            _module.SelectedKeymapChanged += OnSelectedKeymapChanged;
        }

        public void DoUnload()
        {
            _module.SelectedKeymapChanged -= OnSelectedKeymapChanged;
        }

        private void PopulateDropdown()
        {
            if (_keymapDropdown == null) return;
            _keymapDropdown.ValueChanged -= OnKeymapSelected;
            _keymapDropdown.Items.Clear();

            foreach (var keymap in _module.AvailableKeymaps)
                _keymapDropdown.Items.Add(keymap.Name);

            // Try to select the currently active keymap for convenience.
            string activeId = _module.SelectedKeymapId;
            var active = _module.AvailableKeymaps.FirstOrDefault(k => k.Id == activeId);
            if (active != null)
                _keymapDropdown.SelectedItem = active.Name;
            else if (_keymapDropdown.Items.Count > 0)
                _keymapDropdown.SelectedItem = _keymapDropdown.Items[0];

            _keymapDropdown.ValueChanged += OnKeymapSelected;

            // Trigger initial preview.
            if (_keymapDropdown.SelectedItem != null)
            {
                var initialKeymap = _module.AvailableKeymaps.FirstOrDefault(k => k.Name == _keymapDropdown.SelectedItem);
                UpdatePreview(initialKeymap);
            }
        }

        private void OnScrollValueChanged(object? sender, EventArgs e)
        {
            if (_keybedPanel != null && _scrollTrackBar != null)
                _keybedPanel.HorizontalScrollOffset = (int)_scrollTrackBar.Value;
        }

        private void OnSelectedKeymapChanged(string keymapId)
        {
            var currentName = _keymapDropdown?.SelectedItem;
            var currentKeymap = _module.AvailableKeymaps.FirstOrDefault(k => k.Name == currentName);
            if (currentKeymap?.Id == keymapId) return;

            var keymap = _module.AvailableKeymaps.FirstOrDefault(k => k.Id == keymapId);
            if (keymap == null) return;

            _keymapDropdown!.ValueChanged -= OnKeymapSelected;
            _keymapDropdown.SelectedItem = keymap.Name;
            _keymapDropdown.ValueChanged += OnKeymapSelected;

            UpdatePreview(keymap);
        }

        private void OnKeymapSelected(object? sender, EventArgs e)
        {
            var selectedName = _keymapDropdown?.SelectedItem;
            if (string.IsNullOrEmpty(selectedName))
            {
                UpdatePreview(null);
                return;
            }

            var keymap = _module.AvailableKeymaps.FirstOrDefault(k => k.Name == selectedName);
            if (keymap == null)
            {
                UpdatePreview(null);
                return;
            }

            _module.SelectKeymap(keymap.Id);
            UpdatePreview(keymap);
        }

        private void UpdatePreview(Keymap? keymap)
        {
            if (keymap == null)
            {
                if (_keybedControl != null)
                    _keybedControl.Layout = KeybedLayout.Empty;
                if (_keymapInfoLabel != null)
                    _keymapInfoLabel.Text = "";
                UpdateScrollBar();
                return;
            }

            var layout = KeybedLayoutCalculator.Calculate(keymap);
            if (_keybedControl != null)
                _keybedControl.Layout = layout;

            if (_keymapInfoLabel != null)
            {
                int mappedCount = layout.Keys.Count(k => k.IsMapped);
                string info = mappedCount > 0
                    ? $"Octaves {layout.StartOctave}–{layout.EndOctave}  |  {mappedCount} mapped notes"
                    : $"Octaves {layout.StartOctave}–{layout.EndOctave}  |  empty";
                _keymapInfoLabel.Text = info;
            }

            UpdateScrollBar();
        }

        private void UpdateScrollBar()
        {
            if (_keybedPanel == null || _scrollTrackBar == null || _keybedControl == null) return;

            int maxScroll = Math.Max(0, _keybedControl.Width - _keybedPanel.Width);
            _scrollTrackBar.ValueChanged -= OnScrollValueChanged;
            _scrollTrackBar.MaxValue = maxScroll;
            _scrollTrackBar.Value = 0;
            _scrollTrackBar.ValueChanged += OnScrollValueChanged;
            _keybedPanel.HorizontalScrollOffset = 0;
        }
    }
}
