using NUnit.Framework;
using UnityEngine;
using Core.Services.Purchasing;
using Core.Services.Purchasing.Products;
using System.Reflection;

namespace Core.Editor.Tests
{
    public class MockIAP : IAP
    {
        public string PriceToReturn;
        public void SetInstance() => Instance = this;
        public override string GetProductCost(IAPProductBase product) => PriceToReturn;
        public override bool IsProductOwned(IAPProductBase product) => false;
        public override void RestorePurchases() { }
        protected override void Initialize(EventCallbackCollection callbacks) { }
        protected override void PurchaseProduct(IAPProductBase product) { }
    }

    public class IAPSubscriptionTests
    {
        private MockIAP _mockIAP;
        private IAPSubscription _subscription;

        [SetUp]
        public void SetUp()
        {
            _mockIAP = ScriptableObject.CreateInstance<MockIAP>();
            _mockIAP.SetInstance();
            _subscription = ScriptableObject.CreateInstance<IAPSubscription>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_mockIAP != null) Object.DestroyImmediate(_mockIAP);
            if (_subscription != null) Object.DestroyImmediate(_subscription);
        }

        [Test]
        public void GetPricePerMonth_Divider3_999WithDollar_Returns333()
        {
            _mockIAP.PriceToReturn = "$9.99";
            SetDivider(3);
            Assert.AreEqual("$3.33", _subscription.GetPricePerMonth());
        }

        [Test]
        public void GetPricePerMonth_Divider3_999WithComma_Returns333()
        {
            _mockIAP.PriceToReturn = "$9,99";
            SetDivider(3);
            Assert.AreEqual("$3,33", _subscription.GetPricePerMonth());
        }

        [Test]
        public void GetPricePerMonth_Divider3_5GEL_Returns166GEL()
        {
            _mockIAP.PriceToReturn = "5 GEL";
            SetDivider(3);
            Assert.AreEqual("1.66 GEL", _subscription.GetPricePerMonth());
        }

        [Test]
        public void GetPricePerMonth_Divider3_900_Returns3()
        {
            _mockIAP.PriceToReturn = "$9.00";
            SetDivider(3);
            Assert.AreEqual("$3", _subscription.GetPricePerMonth());
        }

        [Test]
        public void GetPricePerMonth_Divider1_ReturnsSame()
        {
            _mockIAP.PriceToReturn = "$9.99";
            SetDivider(1);
            Assert.AreEqual("$9.99", _subscription.GetPricePerMonth());
        }
        
        [Test]
        public void GetPricePerMonth_ThousandsSeparatorSpace_ReturnsCorrect()
        {
            _mockIAP.PriceToReturn = "1 200,00 €";
            SetDivider(3);
            Assert.AreEqual("400 €", _subscription.GetPricePerMonth());
        }

        [Test]
        public void GetPricePerMonth_ThousandsSeparatorDot_ReturnsCorrect()
        {
            _mockIAP.PriceToReturn = "1.200,00 €";
            SetDivider(3);
            Assert.AreEqual("400 €", _subscription.GetPricePerMonth());
        }

        [Test]
        public void GetPricePerMonth_ByCurrency_TruncatesTo478()
        {
            _mockIAP.PriceToReturn = "BY 4.787";
            SetDivider(1);
            Assert.AreEqual("BY 4.78", _subscription.GetPricePerMonth());
        }

        private void SetDivider(int val)
        {
            var field = typeof(IAPSubscription).GetField("_perMonthDivider", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_subscription, val);
        }
    }
}
