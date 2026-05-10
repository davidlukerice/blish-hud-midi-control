using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Input;

namespace DavidRice.BlishHud.MidiControl.Tests.Input
{
    [TestFixture]
    public class KeyToScanCodeTests
    {
        [TestCase("1", 0x02u)]
        [TestCase("0", 0x0Bu)]
        [TestCase("A", 0x1Eu)]
        [TestCase("a", 0x1Eu)]
        [TestCase("F1", 0x3Bu)]
        [TestCase("f1", 0x3Bu)]
        [TestCase("SPACE", 0x39u)]
        [TestCase("space", 0x39u)]
        [TestCase("ENTER", 0x1Cu)]
        [TestCase("ESC", 0x01u)]
        public void For_KnownKey_ReturnsScanCode(string key, uint expected)
        {
            uint? sc = KeyToScanCode.For(key);

            Assert.That(sc, Is.EqualTo(expected));
        }

        [Test]
        public void For_UnknownKey_ReturnsNull()
        {
            uint? sc = KeyToScanCode.For("UNKNOWN_KEY");
            Assert.That(sc, Is.Null);
        }

        [Test]
        public void For_Null_ReturnsNull()
        {
            uint? sc = KeyToScanCode.For(null!);
            Assert.That(sc, Is.Null);
        }

        [Test]
        public void For_Empty_ReturnsNull()
        {
            uint? sc = KeyToScanCode.For("");
            Assert.That(sc, Is.Null);
        }
    }
}
