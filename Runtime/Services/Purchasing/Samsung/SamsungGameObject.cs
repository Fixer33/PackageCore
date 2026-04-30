using UnityEngine;
#if SAMSUNG_BUILD
using System;
using System.Collections.Generic;
#endif

namespace Core.Services.Purchasing.Samsung
{
    public class SamsungGameObject : MonoBehaviour
    {
        public enum BuildMode
        {
            Development,
            Production
        }

        public enum TestResult
        {
            Success,
            Failure
        }

#if SAMSUNG_BUILD
        private AndroidJavaObject _iapInstance;
        
        public void Initialize(AndroidJavaObject iapInstance, Action initializeFailed, BuildMode buildMode = BuildMode.Production, TestResult testResult = TestResult.Success)
        {
            using AndroidJavaClass cls =
                new AndroidJavaClass("com.samsung.android.sdk.iap.lib.activity.SamsungIAPFragment");

            cls.CallStatic("init", gameObject.name);
            
            _iapInstance = cls.CallStatic<AndroidJavaObject>("getInstance");
            if (_iapInstance == null)
            {
                initializeFailed?.Invoke();
                return;
            }

            if (buildMode == BuildMode.Production)
            {
                SetOperationMode(OperationMode.OPERATION_MODE_PRODUCTION);
            }
            else
            {
                switch (testResult)
                {
                    case TestResult.Success:
                        SetOperationMode(OperationMode.OPERATION_MODE_TEST);
                        break;
                    case TestResult.Failure:
                        SetOperationMode(OperationMode.OPERATION_MODE_TEST_FAILURE);
                        break;
                }
            }
        }
        
        #region SDK Implementation

        private string savedPassthroughParam = "";

        public System.Action<ProductInfoList> onGetProductsDetailsListener;
        public System.Action<PurchasedInfo> onStartPaymentListener;
        public System.Action<ConsumedList> onConsumePurchasedItemListener;
        public System.Action<OwnedProductList> onGetOwenedListListener;
        public System.Action<PromotionEligibilityList> onGetPromotionEligibilityListener;
        public System.Action<PurchasedInfo> onChangeSubscriptionPlanListener;

        #region IAP Functions

        internal void SetOperationMode(OperationMode mode)
        {
            _iapInstance.Call("setOperationMode", mode.ToString());
        }

        internal void GetProductsDetails(string itemIDs, System.Action<ProductInfoList> listener)
        {
            onGetProductsDetailsListener = listener;
            _iapInstance.Call("getProductDetails", itemIDs);
        }

        internal void GetOwnedList(ItemType itemType, System.Action<OwnedProductList> listener)
        {
            onGetOwenedListListener = listener;
            _iapInstance.Call("getOwnedList", itemType.ToString());
        }

        internal void StartPayment(string itemID, string passThroughParam, System.Action<PurchasedInfo> listener)
        {
            savedPassthroughParam = passThroughParam;
            onStartPaymentListener = listener;
            _iapInstance.Call("startPayment", itemID, passThroughParam);
        }

        public void ConsumePurchasedItems(string purchaseIDs, System.Action<ConsumedList> listener)
        {
            onConsumePurchasedItemListener = listener;
            _iapInstance.Call("consumePurchasedItems", purchaseIDs);
        }

        public void GetPromotionEligibility(string itemIDs, System.Action<PromotionEligibilityList> listener)
        {
            onGetPromotionEligibilityListener = listener;
            _iapInstance.Call("getPromotionEligibility", itemIDs);
        }

        public void ChangeSubscriptionPlan(string oldItemID, string newItemID, ProrationMode prorationMode, string passThroughParam, System.Action<PurchasedInfo> listener)
        {
            savedPassthroughParam = passThroughParam;
            onChangeSubscriptionPlanListener = listener;
            _iapInstance.Call("changeSubscriptionPlan", oldItemID, newItemID, prorationMode.ToString(), passThroughParam);
        }

        #endregion

        #region Callback Functions

        public void OnGetProductsDetails(string resultJSON)
        {
            Debug.Log("OnGetProductsDetails : " + resultJSON);
            ProductInfoList productList = JsonUtility.FromJson<ProductInfoList>(resultJSON);
            Debug.Log("OnGetProductsDetails cnt:" + productList.results.Count);
            for (int i = 0; i < productList.results.Count; ++i)
                Debug.Log("onGetProductsDetails: " + productList.results[i].mItemName);

            onGetProductsDetailsListener?.Invoke(productList);
        }

        public void OnGetOwnedProducts(string resultJSON)
        {
            Debug.Log("onGetOwnedProducts");
            OwnedProductList ownedList = JsonUtility.FromJson<OwnedProductList>(resultJSON);
            Debug.Log("onGetOwnedProducts cnt:" + ownedList.results.Count);
            for (int i = 0; i < ownedList.results.Count; ++i)
            {
                Debug.Log("onGetOwnedProducts: " + ownedList.results[i].mItemName);
                setSubscriptionPriceChangeVo(ownedList.results[i]);
            }

            onGetOwenedListListener?.Invoke(ownedList);
        }

        private void setSubscriptionPriceChangeVo(OwnedProductVo ownedItem)
        {
            if (string.IsNullOrEmpty(ownedItem.changeSubscriptionPrices))
            {
                return;
            }

            InnerSubscriptionPriceChangeVo innerSubscriptionPriceChangeVo = JsonUtility.FromJson<InnerSubscriptionPriceChangeVo>(ownedItem.changeSubscriptionPrices);
            ownedItem.changeSubscriptionPrices = "";

            ownedItem.subscriptionPriceChangeVo = new SubscriptionPriceChangeVo(innerSubscriptionPriceChangeVo);
        }

        public void OnConsumePurchasedItems(string resultJSON)
        {
            Debug.Log("OnConsumePurchasedItems : " + resultJSON);
            ConsumedList consumedList = JsonUtility.FromJson<ConsumedList>(resultJSON);
            Debug.Log("OnConsumePurchasedItems cnt:" + consumedList.results.Count);
            for (int i = 0; i < consumedList.results.Count; ++i)
            {
                Debug.Log("OnConsumePurchasedItems: " + consumedList.results[i].mPurchaseId);
            }

            onConsumePurchasedItemListener?.Invoke(consumedList);
        }

        public void OnPayment(string resultJSON)
        {
            Debug.Log("onPayment: " + resultJSON);
            PurchasedInfo purchasedInfo = JsonUtility.FromJson<PurchasedInfo>(resultJSON);
               
			if( purchasedInfo != null )
			{
				if( purchasedInfo.results.mPassThroughParam != savedPassthroughParam )
		            Debug.Log("PassThroughParam is different!!!");
			}

            onStartPaymentListener?.Invoke(purchasedInfo);
        }

        public void OnGetPromotionEligibility(string resultJSON)
        {
            Debug.Log("onGetPromotionEligibility : " + resultJSON);
            PromotionEligibilityList promotionEligibilityList = JsonUtility.FromJson<PromotionEligibilityList>(resultJSON);
            Debug.Log("onGetPromotionEligibility cnt:" + promotionEligibilityList.results.Count);
            for (int i = 0; i < promotionEligibilityList.results.Count; ++i)
            {
                Debug.Log("onGetPromotionEligibility: " + promotionEligibilityList.results[i].itemId);
            }

            onGetPromotionEligibilityListener?.Invoke(promotionEligibilityList);
        }

        public void OnChangeSubscriptionPlan(string resultJSON)
        {
            Debug.Log("onChangeSubscriptionPlan: " + resultJSON);
            PurchasedInfo purchasedInfo = JsonUtility.FromJson<PurchasedInfo>(resultJSON);

            if (purchasedInfo != null)
            {
                if (purchasedInfo.results.mPassThroughParam != savedPassthroughParam)
                    Debug.Log("PassThroughParam is different!!!");
            }

            onChangeSubscriptionPlanListener?.Invoke(purchasedInfo);
        }

        #endregion
        
        #region Value Objects

        public enum OperationMode
        {
            OPERATION_MODE_TEST_FAILURE,
            OPERATION_MODE_PRODUCTION,
            OPERATION_MODE_TEST
        }

        public enum ItemType
        {
            item,
            subscription,
            all
        }

        public enum ProrationMode
        {
            INSTANT_PRORATED_DATE,
            INSTANT_PRORATED_CHARGE,
            INSTANT_NO_PRORATION,
            DEFERRED
        }

        public enum PriceChangeMode
        {
            PRICE_INCREASE_USER_AGREEMENT_REQUIRED = 0,
            PRICE_INCREASE_NO_USER_AGREEMENT_REQUIRED = 1,
            PRICE_DECREASE = 2
        }

        [System.Serializable]
        public class ErrorVo
        {
            public int errorCode = 1;
            public string errorString = "";
            public string extraString = "";
        }

        [System.Serializable]
        public class ProductInfoList
        {
            public ErrorVo errorInfo;
            public List<ProductVo> results;
        }

        [System.Serializable]
        public class ProductVo
        {
            public string mItemId = "";
            public string mItemName = "";
            public string mItemPrice = "";
            public string mItemPriceString = "";
            public string mCurrencyUnit = "";
            public string mCurrencyCode = "";
            public string mItemDesc = "";
            public string mType = "";
            public string mConsumableYN = "";
            public string mItemImageUrl = "";
            public string mItemDownloadUrl = "";
            public string mReserved1 = "";
            public string mReserved2 = "";
            public string mFreeTrialPeriod = "";
            public string mSubscriptionDurationUnit = "";
            public string mSubscriptionDurationMultiplier = "";
            public string mTieredPrice = "";
            public string mTieredPriceString = "";
            public string mTieredSubscriptionYN = "";
            public string mTieredSubscriptionDurationUnit = "";
            public string mTieredSubscriptionDurationMultiplier = "";
            public string mTieredSubscriptionCount = "";
            public string mShowStartDate = "";
            public string mShowEndDate = "";
        }

        [System.Serializable]
        public class OwnedProductList
        {
            public ErrorVo errorInfo;
            public List<OwnedProductVo> results;
        }

        [System.Serializable]
        public class OwnedProductVo
        {
            public string mItemId = "";
            public string mItemName = "";
            public string mItemPrice = "";
            public string mItemPriceString = "";
            public string mCurrencyUnit = "";
            public string mCurrencyCode = "";
            public string mItemDesc = "";
            public string mType = "";
            public string mConsumableYN = "";
            public string mSubscriptionEndDate = "";
            public string mPaymentId = "";
            public string mPurchaseId = "";
            public string mPurchaseDate = "";
            public string mPassThroughParam = "";
            public string changeSubscriptionPrices = "";
            public SubscriptionPriceChangeVo subscriptionPriceChangeVo;
        }

        [System.Serializable]
        public class InnerSubscriptionPriceChangeVo
        {
            public string appName = "";
            public string itemName = "";
            public string subscriptionPaymentFreqUnit = "";
            public string subscriptionPaymentFreqN = "";
            public string startDate = "";
            public double originalLocalPrice = 0.0;
            public string originalLocalPriceString = "";
            public double newLocalPrice = 0.0;
            public string newLocalPriceString = "";
            public string isConsented = "N";        // "N" or "Y"
            public string priceChangeMode = "0";    // 0: PRICE_INCREASE_USER_AGREEMENT_REQUIRED, 1: PRICE_INCREASE_NO_USER_AGREEMENT_REQUIRED, 2: PRICE_DECREASE
        }

        [System.Serializable]
        public class SubscriptionPriceChangeVo
        {
            public string appName = "";
            public string itemName = "";
            public string subscriptionPaymentFreqUnit = "";
            public string subscriptionPaymentFreqN = "";
            public string startDate = "";
            public double originalLocalPrice = 0.0;
            public string originalLocalPriceString = "";
            public double newLocalPrice = 0.0;
            public string newLocalPriceString = "";
            public bool isConsented = false;
            public PriceChangeMode priceChangeMode;

            public SubscriptionPriceChangeVo(InnerSubscriptionPriceChangeVo innerVo)
            {
                this.appName = innerVo.appName;
                this.itemName = innerVo.itemName;
                this.subscriptionPaymentFreqUnit = innerVo.subscriptionPaymentFreqUnit;
                this.subscriptionPaymentFreqN = innerVo.subscriptionPaymentFreqN;
                this.startDate = innerVo.startDate;
                this.originalLocalPrice = innerVo.originalLocalPrice;
                this.originalLocalPriceString = innerVo.originalLocalPriceString;
                this.newLocalPrice = innerVo.newLocalPrice;
                this.newLocalPriceString = innerVo.newLocalPriceString;
                this.isConsented = innerVo.isConsented.Equals("Y");
                try
                {
                    this.priceChangeMode = (PriceChangeMode)Convert.ToInt32(innerVo.priceChangeMode);
                }
                catch (FormatException)
                {
                    this.priceChangeMode = PriceChangeMode.PRICE_INCREASE_USER_AGREEMENT_REQUIRED;
                }
            }
        }

        [System.Serializable]
        public class PurchasedInfo
        {
            public ErrorVo errorInfo;
            public PurchaseVo results;
        }

        [System.Serializable]
        public class PurchaseVo
        {
            public string mItemId = "";
            public string mItemName = "";
            public string mItemPrice = "";
            public string mItemPriceString = "";
            public string mCurrencyUnit = "";
            public string mCurrencyCode = "";
            public string mItemDesc = "";
            public string mType = "";
            public string mConsumableYN = "";
            public string mPaymentId = "";
            public string mPurchaseId = "";
            public string mPurchaseDate = "";
            public string mVerifyUrl = "";
            public string mPassThroughParam = "";
            public string mItemImageUrl = "";
            public string mItemDownloadUrl = "";
            public string mReserved1 = "";
            public string mReserved2 = "";
            public string mOrderId = "";
        }

        [System.Serializable]
        public class PromotionEligibilityList
        {
            public ErrorVo errorInfo;
            public List<PromotionEligibilityVo> results;
        }

        [System.Serializable]
        public class PromotionEligibilityVo
        {
            public string itemId = "";
            public string pricing = "";
        }

        [System.Serializable]
        public class ConsumedList
        {
            public ErrorVo errorInfo;
            public List<ConsumeVo> results;
        }

        [System.Serializable]
        public class ConsumeVo
        {
            public string mPurchaseId = "";
            public string mStatusString = "";
            public int mStatusCode = 0;
        }

        #endregion
        #endregion

#endif
    }
}