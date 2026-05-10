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

        public string? ActiveDeviceName => _activeDeviceName;
        public bool IsDeviceOpen => _midiIn != null;

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
                Logger.Info($"Opened MIDI device: {deviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to open MIDI device '{deviceName}': {ex.Message}");
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
                    Logger.Warn($"Exception while closing MIDI device: {ex.Message}");
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
            Close();
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
