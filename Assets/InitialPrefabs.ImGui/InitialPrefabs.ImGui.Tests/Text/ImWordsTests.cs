using NUnit.Framework;

namespace InitialPrefabs.NimGui.Text.Tests {

    public unsafe class ImWordsTests {

        ImWords textBuffer;
        
        [SetUp]
        public void Setup() {
            textBuffer = new ImWords(4096);
            Assert.True(textBuffer.Ptr != null, "Text Buffer not initialized");
            Assert.AreEqual(textBuffer.Capacity, 4096, "Not initialized with set character count");
            Assert.AreEqual(textBuffer.Index, 0, "Initial index not set to 0");
        }

        [TearDown]
        public void TearDown() {
            textBuffer.Dispose();
            Assert.True(textBuffer.Ptr == null, "Text Buffer not released");
        }

        [Test]
        public void RequestingAString() {
            var expected = "Test";
            var content = textBuffer.Request("Test");

            Assert.AreEqual(expected.Length, content.Length, "Content length not equal");
            for (int i = 0; i < expected.Length; ++i) {
                Assert.AreEqual(expected[i], content[i], "Mismatched character.");
            }
        }

        [Test]
        public void Resetting() {
            RequestingAString();
            textBuffer.Reset();
            Assert.AreEqual(0, textBuffer.Index, "Index was not resetted.");
        }
    }
}
