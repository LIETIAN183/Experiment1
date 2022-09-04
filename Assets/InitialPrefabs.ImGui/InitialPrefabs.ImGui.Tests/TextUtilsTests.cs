using InitialPrefabs.NimGui.Text;
using NUnit.Framework;
using UnityEngine;

namespace InitialPrefabs.NimGui.Tests {

    public class TextUtilsTests {

        ImWords words;
        
        [SetUp]
        public void Setup() {
            words = new ImWords(1024);
        }

        [TearDown]
        public void TearDown() {
            words.Dispose();
        }

        [Test]
        public void CountPositiveDigits() {
            var random = Random.Range(0, 9999);
            var expectedLength = $"{random}".Length;

            var digits = TextUtils.CountDigits(random);
            Assert.AreEqual(digits, expectedLength);
        }

        [Test]
        public void CountNegativeDigits() {
            var random = Random.Range(-9999, 0);
            var expectedLength = $"{random}".Length;

            var digits = TextUtils.CountDigits(random);
            Assert.AreEqual(digits, expectedLength);
        }

        [Test]
        public void CountEmptyDigits() {
            Assert.AreEqual(1, TextUtils.CountDigits(0));
        }

        [Test]
        public void ConvertPositiveIntToImString() {
            int value = 10;

            var actual = value.ToImString(ref words);
            var expected = value.ToString();

            for (int i = 0; i < expected.Length; ++i) {
                Assert.AreEqual(expected[i], actual[i], "Mismatched character");
            }
            Assert.AreEqual(expected.Length, actual.Length);
        }

        [Test]
        public void ConvertNegativeIntToImString() {
            var value = -129038;

            var actual = value.ToImString(ref words);
            var expected = value.ToString();

            Assert.AreEqual('-', expected[0], "No negative sign");

            for (int i = 0; i < actual.Length; ++i) {
                Assert.AreEqual(expected[i], actual[i], "Mismatched character");
            }
        }

        [Test]
        public void CountPositiveFloat() {
            var f = 10.25f;
            var actual = TextUtils.CountDigits(f, 2);
            Assert.AreEqual(5, actual, "Miscounted digit");
        }

        [Test]
        public void CountNegativeDigit() {
            var f = -10.25f;
            var actual = TextUtils.CountDigits(f, 2);
            Assert.AreEqual(6, actual, "Miscounted digit");
        }

        [Test]
        public unsafe void ConvertNegativeFloatToImString() {
            var value = -12.92f;
            var actual = value.ToImString(ref words);

            var expected = $"{value}";

            for (int i = 0; i < expected.Length; ++i) {
                Assert.AreEqual(expected[i], actual.Ptr[i]);
            }
            Assert.AreEqual(expected.Length, actual.Length);
        }

        [Test]
        public unsafe void ConvertPositiveFloatToImString() {
            var value = 1230.5213f;
            var actual = value.ToImString(ref words, 4);

            var expected = $"{value}";

            Debug.Log(actual.ToString());

            for (int i = 0; i < expected.Length; ++i) {
                Assert.AreEqual(expected[i], actual.Ptr[i]);
            }
            Assert.AreEqual(expected.Length + 1, actual.Length);
        }
    }
}
