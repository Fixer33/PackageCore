using System;
using System.Collections.Generic;
using CI.WSANative.Common;
using CI.WSANative.Store;
using Core.Services.Purchasing.Products;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using Windows.Services.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Core.Services.Purchasing
{
    [CreateAssetMenu(fileName = "Win Store IAP", menuName = "Services/Purchasing/Windows store", order = 0)]
    public class WinStoreIAP : IAP
    {
        [SerializeField] private int _initializationDelayMs = 1500;
        private Dictionary<string, WSAStoreProduct> _products = new Dictionary<string, WSAStoreProduct>();
        private EventCallbackCollection _callbacks;
        
        public override string GetProductCost(IAPProductBase product)
        {
            string id = product.GetId();

            if (_products.TryGetValue(id, out WSAStoreProduct productData) == false)
                return "no-product-found";

            return productData.FormattedPrice;
        }

        public override bool IsProductOwned(IAPProductBase product)
        {
            string id = product.GetId();

            if (_products.TryGetValue(id, out WSAStoreProduct productData) == false)
                return false;

            return productData.IsInUserCollection;
        }

        public override void RestorePurchases()
        {
            Debug.LogWarning("Restore is not supported by WSA Native");
        }

        protected override async void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            _callbacks = callbacksToInvoke;

            bool isHadled = false;
            Dictionary<string, WSAStoreLicense> ownedProducts;
            try
            {
                await UniTask.Delay(_initializationDelayMs);
                WSANativeCore.Initialise();
                var license = WSANativeStore.GetAppLicense();
            }
            catch (Exception e)
            {
                _callbacks.InitializeFailed?.Invoke(e.Message);
                return;
            }
            
            try
            {
                await UniTask.Delay(100);
                
                WSANativeStore.GetAddOns(response =>
                {
                    _products = response.Products;

                    foreach (var premiumProduct in _premiumProducts)
                    {
                        if (_products.ContainsKey(premiumProduct.GetId()) == false)
                        {
                            _callbacks.InitializeFailed?.Invoke($"Product {premiumProduct.GetId()} does not exist in the store");
                            return;
                        }
                    }
                
                    _callbacks.Initialized?.Invoke();
                    isHadled = true;
                });
            }
            catch (Exception e)
            {
                _callbacks.InitializeFailed?.Invoke(e.Message);
                return;
            }
        
            if (isHadled)
                return;
        
#if UNITY_EDITOR
            bool isPurchased =
                UnityEditor.EditorUtility.DisplayDialog("Confirm initialization", "Is initialization successful?", "Success", "Cancel");     
            if (isPurchased == false)
            {
                _callbacks.InitializeFailed?.Invoke("Initialization is set to fail");
                return;
            }
        
            foreach (var iapProductBase in _premiumProducts)
            {
                _products.Add(iapProductBase.GetId(), new WSAStoreProduct()
                {
                    Description = "Debug product",
                    FormattedPrice = "debug1$",
                    IsInUserCollection = false,
                    Title = "Debug product",
                    StoreId = iapProductBase.GetId(),
                });
            }
            _callbacks.Initialized?.Invoke();
#endif
        }

        protected override void PurchaseProduct(IAPProductBase product)
        {
            bool isHadled = false;
            try
            {
                WSANativeStore.RequestPurchase(product.GetId(), result =>
                {
                    switch (result.Status)
                    {
                        case WSAStorePurchaseStatus.AlreadyPurchased:
                        case WSAStorePurchaseStatus.Succeeded:
                            _isDebugPremium = true;
                            _callbacks.ProductPurchased?.Invoke(product);
                            break;
                        default:
                            Exception exception = result.Error ?? new Exception(result.Status.ToString());
                            _callbacks.ProductPurchaseFailed?.Invoke(product, exception.Message);
                            break;
                    }
                    isHadled = true;
                });
            }
            catch (Exception e)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, e.Message);
                isHadled = true;
            }
        
            if (isHadled)
                return;
        
#if UNITY_EDITOR
            bool isPurchased =
                UnityEditor.EditorUtility.DisplayDialog("Purchase confirm", "Is purchase successful?", "Success", "Cancel");     
            if (isPurchased == false)
            {
                _callbacks.ProductPurchaseFailed?.Invoke( product, "User cancelled");
                return;
            }
        
            _isDebugPremium = true;
            _callbacks.ProductPurchased?.Invoke(product);
#endif
        }
    }
}
