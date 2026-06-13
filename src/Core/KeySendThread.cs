#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DavidRice.BlishHud.MidiControl.Core
{
    public enum KeyEventType
    {
        KeyTap = 0,
        KeyDown,
        KeyUp
    }

    public readonly struct SendAction
    {
        public uint ScanCode { get; }
        public int DelayAfterMs { get; }
        public KeyEventType EventType { get; }

        public SendAction(uint scanCode, int delayAfterMs = 0, KeyEventType eventType = KeyEventType.KeyTap)
        {
            ScanCode = scanCode;
            DelayAfterMs = delayAfterMs;
            EventType = eventType;
        }
    }

    public class KeySendThread : IDisposable
    {
        private readonly BlockingCollection<SendAction> _queue;
        private readonly Thread _thread;
        private readonly Action<SendAction> _sendAction;
        private bool _disposed;

        public KeySendThread(Action<SendAction> sendAction)
        {
            _sendAction = sendAction;
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
                _sendAction(action);

                if (action.DelayAfterMs > 0)
                    Thread.Sleep(action.DelayAfterMs);
            }
        }
    }
}
