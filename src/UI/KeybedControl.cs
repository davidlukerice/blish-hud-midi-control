#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using DavidRice.BlishHud.MidiControl.Keymaps.Visualization;

namespace DavidRice.BlishHud.MidiControl.UI
{
    /// <summary>
    /// Custom control that renders a <see cref="KeybedLayout"/> as a row of piano keys.
    /// </summary>
    public class KeybedControl : Control
    {
        private static readonly Logger Logger = Logger.GetLogger<KeybedControl>();

        private const int WhiteKeyWidth = 24;
        private const int KeyPadding = 4;
        private const int DefaultWidth = 420;
        private const int NoteLabelHeight = 14;
        private const int DefaultHeight = 110;

        private KeybedLayout _layout = KeybedLayout.Empty;
        private KeybedKey? _hoveredKey;
        private readonly Dictionary<int, Rectangle> _keyRects = new Dictionary<int, Rectangle>();
        private static Texture2D? _pixelTexture;

        public KeybedControl()
        {
            Size = new Point(DefaultWidth, DefaultHeight);
        }

        public KeybedLayout Layout
        {
            get => _layout;
            set
            {
                if (_layout == value) return;
                _layout = value;

                if (_layout.IsEmpty)
                {
                    Size = new Point(DefaultWidth, DefaultHeight);
                }
                else
                {
                    int whiteKeyCount = _layout.Keys.Count(k => !k.IsBlackKey);
                    int desiredWidth = whiteKeyCount * WhiteKeyWidth + KeyPadding * 2;
                    Size = new Point(desiredWidth, DefaultHeight);
                }

                RebuildKeyRects();
                Invalidate();
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            UpdateHoveredKey();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            UpdateHoveredKey();
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);
            if (_hoveredKey != null)
            {
                _hoveredKey = null;
                BasicTooltipText = "";
                Invalidate();
            }
        }

        private void UpdateHoveredKey()
        {
            KeybedKey? newHover = null;
            var mousePos = this.RelativeMousePosition;

            foreach (var key in _layout.Keys)
            {
                if (!_keyRects.TryGetValue(key.NoteNumber, out var rect))
                    continue;

                if (rect.Contains(mousePos))
                {
                    newHover = key;
                    break;
                }
            }

            if (_hoveredKey?.NoteNumber != newHover?.NoteNumber)
            {
                _hoveredKey = newHover;
                BasicTooltipText = BuildTooltipText(newHover);
                Invalidate();
            }
        }

        private static string BuildTooltipText(KeybedKey? key)
        {
            if (key == null) return "";

            if (!key.IsMapped)
                return $"{key.NoteName} (unmapped)";

            if (key.IsKeySwitch)
                return $"{key.NoteName}\r\nOctave shift (Key: {key.Gw2Key})";

            var sb = new StringBuilder();
            sb.AppendLine(key.NoteName);
            sb.AppendLine($"GW2 Key: {key.Gw2Key}");
            if (key.Octave.HasValue)
                sb.AppendLine($"Octave: {key.Octave.Value}");

            if (key.HasAltOctave && key.AltOctave.HasValue && !string.IsNullOrEmpty(key.AltOctaveKey))
                sb.AppendLine($"Also plays as {key.AltOctaveKey} on octave {key.AltOctave.Value}");

            return sb.ToString().TrimEnd();
        }

        private void RebuildKeyRects()
        {
            _keyRects.Clear();
            if (_layout.IsEmpty) return;

            int whiteKeyCount = _layout.Keys.Count(k => !k.IsBlackKey);
            float whiteKeyWidth = WhiteKeyWidth;
            float blackKeyWidth = whiteKeyWidth * 0.65f;
            int availableHeight = DefaultHeight - KeyPadding * 2 - NoteLabelHeight;
            int whiteKeyHeight = availableHeight;
            int blackKeyHeight = (int)(availableHeight * 0.6f);

            float x = KeyPadding;
            int y = KeyPadding;

            var whiteRects = new Dictionary<int, Rectangle>();

            foreach (var key in _layout.Keys)
            {
                if (key.IsBlackKey) continue;
                var rect = new Rectangle((int)x, y, (int)whiteKeyWidth, whiteKeyHeight);
                _keyRects[key.NoteNumber] = rect;
                whiteRects[key.NoteNumber] = rect;
                x += whiteKeyWidth;
            }

            foreach (var key in _layout.Keys)
            {
                if (!key.IsBlackKey) continue;
                int preceding = GetPrecedingWhiteKeyIndex(key.NoteNumber % 12);
                if (preceding < 0) continue;

                int prevNote = key.NoteNumber - 1;
                if (!whiteRects.TryGetValue(prevNote, out var prevRect)) continue;

                float blackX = prevRect.Right - blackKeyWidth / 2f;
                var rect = new Rectangle((int)blackX, y, (int)blackKeyWidth, blackKeyHeight);
                _keyRects[key.NoteNumber] = rect;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // bounds arrives in local coordinates (origin 0,0). Blish HUD's scissor is in screen
            // space and there is no implicit local-to-screen transform inside SpriteBatch.Begin,
            // so we must offset to the control's actual position on screen before drawing.
            bounds.Offset(this.AbsoluteBounds.X, this.AbsoluteBounds.Y);

            if (_layout.IsEmpty)
            {
                DrawEmptyMessage(spriteBatch, bounds);
                return;
            }

            DrawKeys(spriteBatch, bounds);
        }

        private static void DrawEmptyMessage(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var font = GameService.Content.DefaultFont16;
            const string text = "No keymap to preview.";
            var size = font.MeasureString(text);
            float x = bounds.X + (bounds.Width - size.Width) / 2f;
            float y = bounds.Y + (bounds.Height - size.Height) / 2f;
            spriteBatch.DrawString(font, text, new Vector2(x, y), Color.Gray);
        }

        private static void DrawKeys(SpriteBatch spriteBatch, Rectangle bounds, KeybedLayout layout, KeybedKey? hoveredKey, Dictionary<int, Rectangle> keyRects)
        {
            var font = GameService.Content.DefaultFont12;

            int whiteKeyCount = layout.Keys.Count(k => !k.IsBlackKey);
            if (whiteKeyCount == 0) return;

            int availableWidth = bounds.Width - KeyPadding * 2;
            int availableHeight = bounds.Height - KeyPadding * 2 - NoteLabelHeight;
            float whiteKeyWidth = availableWidth / (float)whiteKeyCount;
            float blackKeyWidth = whiteKeyWidth * 0.65f;
            int whiteKeyHeight = availableHeight;
            int blackKeyHeight = (int)(availableHeight * 0.6f);

            var whiteKeyRects = new Dictionary<int, Rectangle>();
            var cNoteRects = new List<(string NoteName, Rectangle Rect)>();
            float x = bounds.X + KeyPadding;
            int y = bounds.Y + KeyPadding;

            // First pass: white keys
            foreach (var key in layout.Keys)
            {
                if (key.IsBlackKey) continue;

                var rect = new Rectangle((int)x, y, (int)whiteKeyWidth, whiteKeyHeight);
                whiteKeyRects[key.NoteNumber] = rect;

                if (key.NoteNumber % 12 == 0)
                    cNoteRects.Add((key.NoteName, rect));

                Color fillColor = key.IsMapped
                    ? Color.FromNonPremultiplied(245, 245, 245, 255)
                    : Color.FromNonPremultiplied(190, 190, 190, 255);
                Color borderColor = key.IsKeySwitch
                    ? Color.Orange
                    : Color.FromNonPremultiplied(120, 120, 120, 255);

                DrawFilledRect(spriteBatch, rect, fillColor);
                DrawRectBorder(spriteBatch, rect, borderColor, 1);

                if (key.IsMapped && key.Gw2Key != null)
                {
                    var gw2Rect = new Rectangle(rect.X, rect.Y + (int)(rect.Height * 0.55f), rect.Width, (int)(rect.Height * 0.45f));
                    DrawCenteredString(spriteBatch, font, key.Gw2Key, gw2Rect, Color.Black);
                }

                x += whiteKeyWidth;
            }

            // Second pass: black keys overlaid between white keys
            foreach (var key in layout.Keys)
            {
                if (!key.IsBlackKey) continue;

                int preceding = GetPrecedingWhiteKeyIndex(key.NoteNumber % 12);
                if (preceding < 0 || !whiteKeyRects.TryGetValue(key.NoteNumber - 1, out var prevWhiteRect))
                    continue;

                float blackX = prevWhiteRect.Right - blackKeyWidth / 2f;
                var rect = new Rectangle((int)blackX, y, (int)blackKeyWidth, blackKeyHeight);

                Color fillColor = key.IsMapped
                    ? Color.FromNonPremultiplied(30, 30, 30, 255)
                    : Color.FromNonPremultiplied(70, 70, 70, 255);
                Color borderColor = key.IsKeySwitch
                    ? Color.Orange
                    : Color.FromNonPremultiplied(50, 50, 50, 255);

                DrawFilledRect(spriteBatch, rect, fillColor);
                DrawRectBorder(spriteBatch, rect, borderColor, 1);

                if (key.IsMapped && key.Gw2Key != null)
                {
                    DrawCenteredString(spriteBatch, font, key.Gw2Key, rect, Color.White);
                }
            }

            // Third pass: hover highlight overlay
            if (hoveredKey != null && keyRects.TryGetValue(hoveredKey.NoteNumber, out var hoverLocalRect))
            {
                var hoverScreenRect = new Rectangle(
                    bounds.X + KeyPadding + hoverLocalRect.X - KeyPadding,
                    bounds.Y + KeyPadding + hoverLocalRect.Y - KeyPadding,
                    hoverLocalRect.Width,
                    hoverLocalRect.Height);

                var tint = hoveredKey.IsMapped
                    ? new Color(255, 165, 0, 77)
                    : new Color(176, 196, 222, 77);
                DrawFilledRect(spriteBatch, hoverScreenRect, tint);
            }

            // Fourth pass: C note labels below the keys (octave reference)
            int labelY = bounds.Y + KeyPadding + availableHeight + 2;
            foreach (var (noteName, rect) in cNoteRects)
            {
                var labelRect = new Rectangle(rect.X, labelY, rect.Width, NoteLabelHeight - 2);
                DrawCenteredString(spriteBatch, font, noteName, labelRect, Color.LightGray);
            }
        }

        private void DrawKeys(SpriteBatch spriteBatch, Rectangle bounds)
        {
            DrawKeys(spriteBatch, bounds, _layout, _hoveredKey, _keyRects);
        }

        private static int GetPrecedingWhiteKeyIndex(int semitone) => semitone switch
        {
            1 => 0,  // C#
            3 => 1,  // D#
            6 => 3,  // F#
            8 => 4,  // G#
            10 => 5, // A#
            _ => -1
        };

        private static void DrawFilledRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(GetPixelTexture(), rect, color);
        }

        private static void DrawRectBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            var px = GetPixelTexture();
            // Top
            spriteBatch.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(px, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Left
            spriteBatch.Draw(px, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(px, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        private static void DrawCenteredString(SpriteBatch spriteBatch, BitmapFont font, string text, Rectangle rect, Color color)
        {
            var size = font.MeasureString(text);
            float x = rect.X + (rect.Width - size.Width) / 2f;
            float y = rect.Y + (rect.Height - size.Height) / 2f;
            spriteBatch.DrawString(font, text, new Vector2(x, y), color);
        }

        private static Texture2D GetPixelTexture()
        {
            if (_pixelTexture != null) return _pixelTexture;
            var device = GameService.Graphics.GraphicsDeviceManager.GraphicsDevice;
            _pixelTexture = new Texture2D(device, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            return _pixelTexture;
        }
    }
}
