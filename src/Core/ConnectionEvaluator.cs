#nullable enable

using System;
using System.Collections.Generic;

namespace DavidRice.BlishHud.MidiControl.Core
{
    /// <summary>
    /// Pure functions for evaluating MIDI device connection state and deciding on actions.
    /// Separated from <see cref="MidiInputManager"/> so unit tests can call them without
    /// loading the Blish HUD runtime.
    /// </summary>
    public static class ConnectionEvaluator
    {
        public enum Action
        {
            NoAction,
            Close,
            Reopen
        }

        /// <summary>
        /// Given a target device name, the currently available devices, and whether we
        /// believe a device is open, decide what action to take.
        /// </summary>
        public static Action Evaluate(
            string? targetDeviceName,
            IReadOnlyList<string> availableDevices,
            bool isDeviceOpen)
        {
            if (string.IsNullOrEmpty(targetDeviceName))
                return Action.NoAction;

            bool deviceAvailable = Contains(availableDevices, targetDeviceName!);

            if (isDeviceOpen && !deviceAvailable)
                return Action.Close;

            if (!isDeviceOpen && deviceAvailable)
                return Action.Reopen;

            return Action.NoAction;
        }

        private static bool Contains(IReadOnlyList<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                    return true;
            }
            return false;
        }
    }
}
