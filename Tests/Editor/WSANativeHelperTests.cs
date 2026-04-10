using CI.WSANative.Common;
using NUnit.Framework;

namespace Core.Editor.Tests
{
    [TestFixture]
    public class WSANativeHelperTests
    {
        [TestCase("0.00$", true)]
        [TestCase("0,000t", true)]
        [TestCase("$0.00", true)]
        [TestCase("0", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("9.99$", false)]
        [TestCase("$10.00", false)]
        [TestCase("1,50€", false)]
        [TestCase("0.01", false)]
        [TestCase("Free", true)] // No digits at all, treated as zero by current logic
        [TestCase("9$", false)]
        public void IsPriceZero_CorrectlyIdentifiesZeroPrice(string input, bool expected)
        {
            Assert.AreEqual(expected, WSANativeHelper.IsPriceZero(input));
        }
    }
}
