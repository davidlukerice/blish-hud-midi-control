using System.IO;
using System.Linq;
using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps
{
    [TestFixture]
    public class KeymapRegistryTests
    {
        [Test]
        public void ConstructorRegistersAllBuiltInKeymaps()
        {
            var registry = new KeymapRegistry();

            Assert.That(registry.AllKeymaps.Count, Is.EqualTo(14));
        }

        [Test]
        public void ConstructorRegistersGeneral()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("general");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("General (Manual)"));
        }

        [Test]
        public void ConstructorRegistersMinstrelAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("minstrel-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("The Minstrel (Auto)"));
        }

        [Test]
        public void ConstructorRegistersGrandPianoAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("grand-piano-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Ornate Grand Piano (Auto)"));
        }

        [Test]
        public void ConstructorRegistersChoirBellAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("choir-bell-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Magnanimous Choir Bell (Auto)"));
        }

        [Test]
        public void ConstructorRegistersFluteCAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("flute-c-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Flute (C) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersFluteEAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("flute-e-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Flute (E) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersLuteAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("lute-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Lute (Auto)"));
        }

        [Test]
        public void ConstructorRegistersHarpAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("harp-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Harp (Auto)"));
        }

        [Test]
        public void ConstructorRegistersHornCAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("horn-c-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Horn (C) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersBassGuitarAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("bass-guitar-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Bass Guitar (Auto)"));
        }

        [Test]
        public void ConstructorRegistersVerdarachAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("verdarach-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Verdarach (Auto)"));
        }

        [Test]
        public void ConstructorRegistersHornEAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("horn-e-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Horn (E) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersFrameDrumAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("frame-drum-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Frame Drum (Auto)"));
        }

        [Test]
        public void ConstructorRegistersMinstrelNonAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("minstrel");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("The Minstrel"));
        }

        [Test]
        public void FindById_ReturnsMatch()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("minstrel-auto");

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("The Minstrel (Auto)"));
        }

        [Test]
        public void FindById_ReturnsNull_WhenNotFound()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("nonexistent");

            Assert.That(found, Is.Null);
        }

        [Test]
        public void FindByName_ReturnsMatch()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindByName("The Minstrel (Auto)");

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo("minstrel-auto"));
        }

        [Test]
        public void FindByName_ReturnsNull_WhenNotFound()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindByName("Nonexistent Keymap");

            Assert.That(found, Is.Null);
        }

        [Test]
        public void AllKeymaps_BuiltInComeBeforeCustom()
        {
            var registry = new KeymapRegistry();
            registry.Register(new Keymap(id: "custom-1", name: "Custom 1"));

            var all = registry.AllKeymaps;
            Assert.That(all.Last().Id, Is.EqualTo("custom-1"));
        }

        [Test]
        public void CustomKeymapCount_InitiallyZero()
        {
            var registry = new KeymapRegistry();
            Assert.That(registry.CustomKeymapCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadCustomKeymaps_LoadsValidFile()
        {
            var registry = new KeymapRegistry();
            string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempDir);
            try
            {
                string json = "{\"id\":\"test-map\",\"name\":\"Test Map\",\"notes\":{}}";
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempDir, "test.json"), json);

                registry.LoadCustomKeymaps(tempDir);

                Assert.That(registry.CustomKeymapCount, Is.EqualTo(1));
                Assert.That(registry.FindById("test-map"), Is.Not.Null);
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void LoadCustomKeymaps_IsIdempotent_OnRescan()
        {
            var registry = new KeymapRegistry();
            string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempDir);
            try
            {
                string json = "{\"id\":\"test-map\",\"name\":\"Test Map\",\"notes\":{}}";
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempDir, "test.json"), json);

                registry.LoadCustomKeymaps(tempDir);
                registry.LoadCustomKeymaps(tempDir);

                Assert.That(registry.CustomKeymapCount, Is.EqualTo(1));
                Assert.That(registry.AllKeymaps.Count, Is.EqualTo(15)); // 14 built-in + 1 custom
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void LoadCustomKeymaps_ClearsCustoms_AndErrors_OnRescan()
        {
            var registry = new KeymapRegistry();
            string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempDir);
            try
            {
                // First scan: valid file
                string valid = "{\"id\":\"test-map\",\"name\":\"Test Map\",\"notes\":{}}";
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempDir, "test.json"), valid);

                registry.LoadCustomKeymaps(tempDir);
                Assert.That(registry.CustomKeymapCount, Is.EqualTo(1));
                Assert.That(registry.LoadErrors.Count, Is.EqualTo(0));

                // Delete valid, add invalid
                System.IO.File.Delete(System.IO.Path.Combine(tempDir, "test.json"));
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempDir, "bad.json"), "not json");

                registry.LoadCustomKeymaps(tempDir);
                Assert.That(registry.CustomKeymapCount, Is.EqualTo(0));
                Assert.That(registry.LoadErrors.Count, Is.GreaterThan(0));
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void Register_AddsKeymap()
        {
            var registry = new KeymapRegistry();
            var custom = new Keymap(id: "custom-1", name: "My Custom Keymap");

            registry.Register(custom);

            Assert.That(registry.AllKeymaps.Count, Is.EqualTo(15));
            Assert.That(registry.FindById("custom-1"), Is.SameAs(custom));
        }
    }
}
