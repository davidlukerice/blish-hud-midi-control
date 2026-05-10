using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Core;

namespace DavidRice.BlishHud.MidiControl.Tests.Core
{
    [TestFixture]
    public class KeySendThreadTests
    {
        [Test]
        public void EnqueuedActions_AreProcessedInOrder()
        {
            var processed = new List<uint>();
            using var thread = new KeySendThread(scanCode => processed.Add(scanCode));

            thread.Start();
            thread.Enqueue(new SendAction(0x02));
            thread.Enqueue(new SendAction(0x03));
            thread.Enqueue(new SendAction(0x04));

            thread.Shutdown();

            Assert.That(processed, Is.EqualTo(new[] { 0x02u, 0x03u, 0x04u }));
        }

        [Test]
        public void Shutdown_WaitsForPendingAction()
        {
            var gate = new ManualResetEventSlim();
            var processed = new List<uint>();

            using var thread = new KeySendThread(scanCode =>
            {
                processed.Add(scanCode);
                gate.Set();
            });

            thread.Start();
            thread.Enqueue(new SendAction(0x02));

            Assert.That(gate.Wait(TimeSpan.FromSeconds(2)), Is.True);
            thread.Shutdown();

            Assert.That(processed, Is.EqualTo(new[] { 0x02u }));
        }

        [Test]
        public void Shutdown_StopsThreadCleanly()
        {
            using var thread = new KeySendThread(_ => { });
            thread.Start();

            Assert.That(thread.IsAlive, Is.True);

            thread.Shutdown();

            Assert.That(thread.IsAlive, Is.False);
        }

        [Test]
        public void DelayAfterMs_CausesSleepBetweenActions()
        {
            var stopwatch = Stopwatch.StartNew();
            var timestamps = new List<long>();

            using var thread = new KeySendThread(_ => timestamps.Add(stopwatch.ElapsedMilliseconds));

            thread.Start();
            thread.Enqueue(new SendAction(0x02, delayAfterMs: 75));
            thread.Enqueue(new SendAction(0x03));

            thread.Shutdown();

            Assert.That(timestamps.Count, Is.EqualTo(2));

            var gap = timestamps[1] - timestamps[0];
            Assert.That(gap, Is.GreaterThan(50));
        }

        [Test]
        public void EnqueueAfterShutdown_IsIgnored()
        {
            var processed = new List<uint>();
            using var thread = new KeySendThread(scanCode => processed.Add(scanCode));

            thread.Start();
            thread.Shutdown();

            // After shutdown, this should be a no-op rather than throwing.
            thread.Enqueue(new SendAction(0x02));

            Thread.Sleep(50);

            Assert.That(processed, Is.Empty);
        }

        [Test]
        public void DoubleShutdown_DoesNotThrow()
        {
            using var thread = new KeySendThread(_ => { });
            thread.Start();
            thread.Shutdown();

            Assert.DoesNotThrow(() => thread.Shutdown());
        }
    }
}
