using System;
using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Input;

namespace DavidRice.BlishHud.MidiControl.Tests.Input
{
    [TestFixture]
    public class SendInputApiTests
    {
        [Test]
        public void SendKeyTap_Throws_On_ZeroScanCode()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                SendInputApi.SendKeyTap(scanCode: 0));

            Assert.That(ex!.ParamName, Is.EqualTo("scanCode"));
        }

        [Test]
        public void SendKeyTap_Throws_On_ScanCodeAboveUInt16Max()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                SendInputApi.SendKeyTap(scanCode: (uint)ushort.MaxValue + 1));

            Assert.That(ex!.ParamName, Is.EqualTo("scanCode"));
        }

        [Test]
        public void SendKeyUp_Throws_On_ZeroScanCode()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                SendInputApi.SendKeyUp(scanCode: 0));

            Assert.That(ex!.ParamName, Is.EqualTo("scanCode"));
        }

        [Test]
        public void SendKeyUp_Throws_On_ScanCodeAboveUInt16Max()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                SendInputApi.SendKeyUp(scanCode: (uint)ushort.MaxValue + 1));

            Assert.That(ex!.ParamName, Is.EqualTo("scanCode"));
        }

        [Test]
        public void InputStructSize_MatchesNativeSize()
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf<INPUT>();

            // On x64, INPUT is 40 bytes (4 for type + 36 for union).
            // On x86 it would be 28, but our project targets x64.
            Assert.That(size, Is.EqualTo(40));
        }
    }
}
