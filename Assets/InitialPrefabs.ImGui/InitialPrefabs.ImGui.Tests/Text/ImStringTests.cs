using NUnit.Framework;

namespace InitialPrefabs.NimGui.Text.Tests {

    public unsafe class ImStringTests {

        [Test]
        public void CreatingImString() {
            var text = "Test";
            var content = new ImString(text);

            Assert.AreEqual(text.Length, content.Length, "Mismatched text length");

            for (int i = 0; i < text.Length; ++i) {
                Assert.AreEqual(text[i], content[i], "Mismatched char");
            }
        }
    }
}
