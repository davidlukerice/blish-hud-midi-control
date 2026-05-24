#nullable enable

using System;
using System.Collections.Generic;

namespace DavidRice.BlishHud.MidiControl.Input
{
    /// <summary>
    /// Maps human-readable key strings (e.g. "1", "A", "F1") to Win32 scan codes
    /// for use with <see cref="SendInputApi"/>.
    /// </summary>
    public static class KeyToScanCode
    {
        private static readonly Dictionary<string, uint> Map =
            new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
            {
                // Number row (US QWERTY)
                ["1"] = 0x02, ["2"] = 0x03, ["3"] = 0x04, ["4"] = 0x05, ["5"] = 0x06,
                ["6"] = 0x07, ["7"] = 0x08, ["8"] = 0x09, ["9"] = 0x0A, ["0"] = 0x0B,

                // Letters (US QWERTY)
                ["A"] = 0x1E, ["B"] = 0x30, ["C"] = 0x2E, ["D"] = 0x20, ["E"] = 0x12,
                ["F"] = 0x21, ["G"] = 0x22, ["H"] = 0x23, ["I"] = 0x17, ["J"] = 0x24,
                ["K"] = 0x25, ["L"] = 0x26, ["M"] = 0x32, ["N"] = 0x31, ["O"] = 0x18,
                ["P"] = 0x19, ["Q"] = 0x10, ["R"] = 0x13, ["S"] = 0x1F, ["T"] = 0x14,
                ["U"] = 0x16, ["V"] = 0x2F, ["W"] = 0x11, ["X"] = 0x2D, ["Y"] = 0x15,
                ["Z"] = 0x2C,

                // Function keys
                ["F1"] = 0x3B, ["F2"] = 0x3C, ["F3"] = 0x3D, ["F4"] = 0x3E, ["F5"] = 0x3F,
                ["F6"] = 0x40, ["F7"] = 0x41, ["F8"] = 0x42, ["F9"] = 0x43, ["F10"] = 0x44,
                ["F11"] = 0x57, ["F12"] = 0x58,

                // Misc
                ["SPACE"] = 0x39, ["ENTER"] = 0x1C, ["ESC"] = 0x01, ["ESCAPE"] = 0x01,
                ["TAB"] = 0x0F, ["BACKSPACE"] = 0x0E,
            };

        /// <summary>
        /// Returns the scan code for <paramref name="key"/>, or <c>null</c> if unknown.
        /// Case-insensitive.
        /// </summary>
        public static uint? For(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (Map.TryGetValue(key, out uint sc))
                return (uint?)sc;

            return null;
        }
        /// <summary>
        /// Returns the first known key name for the given scan code, or <c>null</c> if unknown.
        /// </summary>
        public static string? GetKeyName(uint scanCode)
        {
            foreach (var kvp in Map)
            {
                if (kvp.Value == scanCode)
                    return kvp.Key;
            }
            return null;
        }
    }
}
