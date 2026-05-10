#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading;
using DavidRice.BlishHud.MidiControl.Input;

namespace DavidRice.BlishHud.MidiControl.Core
{
    public readonly struct SendAction
    {
        public uint ScanCode { get; }
        public int DelayAfterMs { get; }

        public SendAction(uint scanCode, int delayAfterMs = 0)
        {
            ScanCode = scanCode;
            DelayAfterMs = delayAfterMs;
        }
    }

    public class KeySendThread : IDisposable
    {
        private readonly BlockingCollection<SendAction> _queue;
        private readonly Thread _thread;
        private readonly Action<uint> _sendTap;
        private bool _disposed;

        public KeySendThread(Action<uint>? sendTap = null)
        {
            _sendTap = sendTap ?? SendInputApi.SendKeyTap;
            _queue = new BlockingCollection<SendAction>();
            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "MIDI Key Send"
            };
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Enqueue(SendAction action)
        {
            if (_disposed || _queue.IsAddingCompleted)
                return;

            try
            {
                _queue.Add(action);
            }
            catch (InvalidOperationException)
            {
                // Collection was completed between the guard and the add.
            }
        }

        public void Shutdown(TimeSpan? timeout = null)
        {
            if (_disposed)
                return;

            _queue.CompleteAdding();
            _disposed = true;

            var wait = timeout ?? TimeSpan.FromSeconds(5);
            _thread.Join(wait);
        }

        public bool IsAlive => _thread.IsAlive;

        public void Dispose()
        {
            Shutdown();
        }

        private void Run()
        {
            foreach (var action in _queue.GetConsumingEnumerable())
            {
                _sendTap(action.ScanCode);

                if (action.DelayAfterMs > 0)
                    Thread.Sleep(action.DelayAfterMs);
            }
        }
    }
}
