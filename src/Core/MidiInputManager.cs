#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NAudio.Midi;

namespace DavidRice.BlishHud.MidiControl.Core
{
    public sealed class MidiInputManager : IDisposable
    {
        private static readonly Blish_HUD.Logger Logger = Blish_HUD.Logger.GetLogger<MidiInputManager>();

        private readonly ConcurrentQueue<MidiNoteEvent> _noteQueue;
        private MidiIn? _midiIn;
        private string? _activeDeviceName;
        private bool _disposed;
        private bool _retryingConnection;
        private DateTime _lastConnectionCheck = DateTime.MinValue;
        private static readonly TimeSpan ConnectionCheckInterval = TimeSpan.FromSeconds(10);

        public string? ActiveDeviceName => _activeDeviceName;
        public bool IsDeviceOpen => _midiIn != null;
        public bool IsRetryingConnection => _retryingConnection;

        public MidiInputManager(ConcurrentQueue<MidiNoteEvent> noteQueue)
        {
            _noteQueue = noteQueue ?? throw new ArgumentNullException(nameof(noteQueue));
        }

        public static IReadOnlyList<string> AvailableDevices
        {
            get
            {
                var devices = new List<string>();
                for (int i = 0; i < MidiIn.NumberOfDevices; i++)
                {
                    devices.Add(MidiIn.DeviceInfo(i).ProductName);
                }
                return devices;
            }
        }

        public static int? GetDeviceIndex(string deviceName)
        {
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                if (MidiIn.DeviceInfo(i).ProductName == deviceName)
                {
                    return i;
                }
            }
            return null;
        }

        public bool Open(string deviceName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MidiInputManager));

            Close();

            int? index = GetDeviceIndex(deviceName);
            if (index == null)
            {
                Logger.Warn($"MIDI device '{deviceName}' not found.");
                return false;
            }

            try
            {
                _midiIn = new MidiIn(index.Value);
                _midiIn.MessageReceived += OnMessageReceived;
                _midiIn.ErrorReceived += OnErrorReceived;
                _midiIn.Start();
                _activeDeviceName = deviceName;
                _retryingConnection = false;
                Logger.Info($"Opened MIDI device: {deviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to open MIDI device '{deviceName}'.", ex);
                _midiIn?.Dispose();
                _midiIn = null;
                return false;
            }
        }

        public void Close()
        {
            if (_midiIn != null)
            {
                try
                {
                    _midiIn.MessageReceived -= OnMessageReceived;
                    _midiIn.ErrorReceived -= OnErrorReceived;
                    _midiIn.Stop();
                    _midiIn.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Exception while closing MIDI device.", ex);
                }
                finally
                {
                    _midiIn = null;
                    _activeDeviceName = null;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _retryingConnection = false;
            Close();
        }

        /// <summary>
        /// Called each frame to check connection health and auto-reconnect when the target
        /// device disappears or reappears.  Throttled to <see cref="ConnectionCheckInterval"/>.
        /// Returns immediately when <paramref name="targetDeviceName"/> is null or empty.
        /// </summary>
        public void CheckConnection(string targetDeviceName)
        {
            if (_disposed)
                return;

            if (string.IsNullOrEmpty(targetDeviceName))
                return;

            var now = DateTime.UtcNow;
            if (now - _lastConnectionCheck < ConnectionCheckInterval)
                return;

            _lastConnectionCheck = now;

            var available = AvailableDevices;
            var action = ConnectionEvaluator.Evaluate(targetDeviceName, available, IsDeviceOpen);

            switch (action)
            {
                case ConnectionEvaluator.Action.Close:
                    Logger.Warn($"MIDI device '{targetDeviceName}' disappeared. Will attempt to reconnect.");
                    _retryingConnection = true;
                    Close();
                    break;

                case ConnectionEvaluator.Action.Reopen:
                    bool reopened = Open(targetDeviceName);
                    if (reopened)
                    {
                        Logger.Info($"MIDI device '{targetDeviceName}' reconnected.");
                    }
                    else
                    {
                        Logger.Warn($"MIDI device '{targetDeviceName}' reappeared but failed to open.");
                    }
                    break;
            }
        }

        private void OnMessageReceived(object? sender, MidiInMessageEventArgs e)
        {
            var midiEvent = e.MidiEvent;

            if (midiEvent is NoteOnEvent noteOn)
            {
                // MIDI note-on with velocity 0 is actually note-off.
                if (noteOn.Velocity > 0)
                {
                    _noteQueue.Enqueue(new MidiNoteEvent(noteOn.NoteNumber, isNoteOn: true));
                }
                else
                {
                    _noteQueue.Enqueue(new MidiNoteEvent(noteOn.NoteNumber, isNoteOn: false));
                }
            }
            // NAudio 2.x does not have a separate NoteOffEvent type.
            // Note-off is represented as NoteOnEvent with velocity 0 (handled above).
        }

        private void OnErrorReceived(object? sender, MidiInMessageEventArgs e)
        {
            Logger.Warn($"MIDI error: raw=0x{e.RawMessage:X8} timestamp={e.Timestamp}");
        }
    }
}
