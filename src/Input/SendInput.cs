using System;
using System.Runtime.InteropServices;

namespace DavidRice.BlishHud.MidiControl.Input
{
    public static class SendInputApi
    {
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_SCANCODE = 0x0008;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static readonly int InputSize = Marshal.SizeOf<INPUT>();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public static void SendKeyTap(uint scanCode)
        {
            ValidateScanCode(scanCode);

            var inputs = new INPUT[2];
            inputs[0] = CreateKeyboardInput(scanCode, keyUp: false);
            inputs[1] = CreateKeyboardInput(scanCode, keyUp: true);

            SendInput((uint)inputs.Length, inputs, InputSize);
        }

        public static void SendKeyUp(uint scanCode)
        {
            ValidateScanCode(scanCode);

            var input = CreateKeyboardInput(scanCode, keyUp: true);
            SendInput(1, new[] { input }, InputSize);
        }

        private static void ValidateScanCode(uint scanCode)
        {
            if (scanCode == 0 || scanCode > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(scanCode), "Scan code must be between 1 and 65535.");
        }

        private static INPUT CreateKeyboardInput(uint scanCode, bool keyUp)
        {
            uint flags = KEYEVENTF_SCANCODE;
            if (keyUp)
                flags |= KEYEVENTF_KEYUP;

            return new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUT_UNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)scanCode,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_UNION
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public INPUT_UNION u;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}
