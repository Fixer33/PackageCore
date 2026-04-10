using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Core.Services.Purchasing;
using Core.Services.Purchasing.Products;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Services.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Services.Purchasing
{
    [CreateAssetMenu(fileName = "Win Store IAP", menuName = "Services/Purchasing/Windows store", order = 0)]
    public class WinStoreIAP : IAP
    {
        [SerializeField] private int _initializationDelayMs = 1500;
        private Dictionary<string, StoreProduct> _products = new Dictionary<string, StoreProduct>();
        private EventCallbackCollection _callbacks;

        public override string GetProductCost(IAPProductBase product)
        {
            string id = product.GetId();

            if (_products.TryGetValue(id, out StoreProduct productData) == false)
                return "no-product-found";

            return productData.FormattedPrice;
        }

        public override bool IsProductOwned(IAPProductBase product)
        {
            string id = product.GetId();

            if (_products.TryGetValue(id, out StoreProduct productData) == false)
                return false;

            return productData.IsInUserCollection;
        }

        public override void RestorePurchases()
        {
            Debug.LogWarning("Restore is not supported by Win Store IAP");
        }

        protected override async void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            _callbacks = callbacksToInvoke;

            try
            {
                await UniTask.Delay(_initializationDelayMs);
                StoreInitialize();
                GetAppLicense();
            }
            catch (Exception e)
            {
                _callbacks.InitializeFailed?.Invoke(e.Message);
                return;
            }

            try
            {
                await UniTask.Delay(100);

                GetAddOns(response =>
                {
                    if (response.Error != null)
                    {
                        _callbacks.InitializeFailed?.Invoke(response.Error.Message);
                        return;
                    }

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
                });
            }
            catch (Exception e)
            {
                _callbacks.InitializeFailed?.Invoke(e.Message);
                return;
            }

#if UNITY_EDITOR
            foreach (var iapProductBase in _premiumProducts)
            {
                if (_products.ContainsKey(iapProductBase.GetId()) == false)
                {
                    _products.Add(iapProductBase.GetId(), new StoreProduct()
                    {
                        Description = "Debug product",
                        FormattedPrice = "debug1$",
                        IsInUserCollection = false,
                        Title = "Debug product",
                        StoreId = iapProductBase.GetId(),
                    });
                }
            }
            _callbacks.Initialized?.Invoke();
#endif
        }

        protected override void PurchaseProduct(IAPProductBase product)
        {
            try
            {
                RequestPurchase(product.GetId(), result =>
                {
                    switch (result.Status)
                    {
                        case StorePurchaseStatus.AlreadyPurchased:
                        case StorePurchaseStatus.Succeeded:
                            _isDebugPremium = true;
                            _callbacks.ProductPurchased?.Invoke(product);
                            break;
                        default:
                            Exception exception = result.Error ?? new Exception(result.Status.ToString());
                            _callbacks.ProductPurchaseFailed?.Invoke(product, exception.Message);
                            break;
                    }
                });
            }
            catch (Exception e)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, e.Message);
            }

#if UNITY_EDITOR
            bool isPurchased =
                UnityEditor.EditorUtility.DisplayDialog("Purchase confirm", "Is purchase successful?", "Success", "Cancel");
            if (isPurchased == false)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, "User cancelled");
                return;
            }

            _isDebugPremium = true;
            _callbacks.ProductPurchased?.Invoke(product);
#endif
        }

        #region Internal Logic

#if ENABLE_WINMD_SUPPORT
        [DllImport("__Internal")]
        private static extern int GetPageContent([MarshalAs(UnmanagedType.IInspectable)] object frame, [MarshalAs(UnmanagedType.IInspectable)] out object pageContent);
        private static SwapChainPanel DxSwapChainPanel { get; set; }
        private static bool _isInitialised;
#endif

        private void StoreInitialize()
        {
#if ENABLE_WINMD_SUPPORT
            if (!_isInitialised)
            {
                RunOnUIThread(() =>
                {
                    object pageContent;
                    var result = GetPageContent(Window.Current.Content, out pageContent);
                    if (result < 0)
                    {
                        Marshal.ThrowExceptionForHR(result);
                    }
                    DxSwapChainPanel = pageContent as SwapChainPanel;
                    _isInitialised = DxSwapChainPanel != null;
                });
            }
#endif
        }

        private StoreAppLicense GetAppLicense()
        {
#if ENABLE_WINMD_SUPPORT
            var result = StoreContext.GetDefault().GetAppLicenseAsync().AsTask().Result;

            return new StoreAppLicense()
            {
                AddOnLicenses = result.AddOnLicenses.ToDictionary(x => x.Key, y => new StoreLicense()
                {
                    ExpirationDate = y.Value.ExpirationDate,
                    InAppOfferToken = y.Value.InAppOfferToken,
                    IsActive = y.Value.IsActive,
                    StoreId = y.Value.SkuStoreId
                }),
                ExpirationDate = result.ExpirationDate,
                IsActive = result.IsActive,
                IsTrial = result.IsTrial,
                StoreId = result.SkuStoreId,
                TrialTimeRemaining = result.TrialTimeRemaining,
                TrialUniqueId = result.TrialUniqueId
            };
#else
            return new StoreAppLicense();
#endif
        }

        private void GetAddOns(Action<StoreProductQueryResult> response)
        {
#if ENABLE_WINMD_SUPPORT
            GetAddOnsAsync(response);
#endif
        }

#if ENABLE_WINMD_SUPPORT
        private async void GetAddOnsAsync(Action<StoreProductQueryResult> response)
        {
            string[] productKinds = { "Durable", "Consumable", "UnmanagedConsumable" };

            StoreProductQueryResult result = await StoreContext.GetDefault().GetAssociatedStoreProductsAsync(productKinds.ToList());

            StoreProductQueryResult storeProductQuery = new StoreProductQueryResult()
            {
                Products = result.Products.ToDictionary(x => x.Key, y =>
                {
                    bool IsZeroPrice(string formattedPrice)
                    {
                        if (string.IsNullOrEmpty(formattedPrice)) return true;
                        return formattedPrice.All(c => !char.IsDigit(c) || c == '0');
                    }

                    var validSku = y.Value.Skus.FirstOrDefault(s => s.Price.FormattedPrice != null && !IsZeroPrice(s.Price.FormattedPrice) && s.SubscriptionInfo == null);
                    if (validSku == null)
                    {
                        validSku = y.Value.Skus.FirstOrDefault(s => s.Price.FormattedPrice != null && !IsZeroPrice(s.Price.FormattedPrice));
                    }

                    if (validSku == null && y.Value.Skus.Count > 1)
                        validSku = y.Value.Skus[1];
                    if (validSku == null)
                        validSku = y.Value.Skus.FirstOrDefault();

                    return new StoreProduct()
                    {
                        Description = y.Value.Description,
                        FormattedPrice = validSku != null ? validSku.Price.FormattedPrice : y.Value.Price.FormattedPrice,
                        InAppOfferToken = y.Value.InAppOfferToken,
                        IsInUserCollection = y.Value.IsInUserCollection,
                        StoreId = y.Value.StoreId,
                        Title = y.Value.Title,
                    };
                }),
                Error = result.ExtendedError
            };

            if (response != null)
            {
                response(storeProductQuery);
            }
        }
#endif

        private void RequestPurchase(string storeId, Action<StorePurchaseResult> response)
        {
#if ENABLE_WINMD_SUPPORT
            RunOnUIThread(async () =>
            {
                StorePurchaseResult result = await StoreContext.GetDefault().RequestPurchaseAsync(storeId);

                StorePurchaseResult storePurchaseResult = new StorePurchaseResult()
                {
                    Error = result.ExtendedError,
                    Status = (StorePurchaseStatus)result.Status
                };

                RunOnAppThread(() =>
                {
                    if (response != null)
                    {
                        response(storePurchaseResult);
                    }
                }, true);
            });
#endif
        }

        private void RunOnUIThread(Action action, bool waitUntilDone = false)
        {
#if ENABLE_WINMD_SUPPORT
            if (UnityEngine.WSA.Application.RunningOnUIThread())
            {
                action();
            }
            else
            {
                UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                {
                    action();
                }, waitUntilDone);
            }
#endif
        }

        private void RunOnAppThread(Action action, bool waitUntilDone = false)
        {
#if ENABLE_WINMD_SUPPORT
            if (UnityEngine.WSA.Application.RunningOnAppThread())
            {
                action();
            }
            else
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    action();
                }, waitUntilDone);
            }
#endif
        }

        public class StoreProduct
        {
            public string Description { get; set; }
            public string InAppOfferToken { get; set; }
            public bool IsInUserCollection { get; set; }
            public string FormattedPrice { get; set; }
            public string StoreId { get; set; }
            public string Title { get; set; }
        }

        public class StorePurchaseResult
        {
            public StorePurchaseStatus Status { get; set; }
            public Exception Error { get; set; }
        }

        public enum StorePurchaseStatus
        {
            Succeeded = 0,
            AlreadyPurchased = 1,
            NotPurchased = 2,
            NetworkError = 3,
            ServerError = 4
        }

        public class StoreProductQueryResult
        {
            public Dictionary<string, StoreProduct> Products { get; set; }
            public Exception Error { get; set; }
        }

        public class StoreLicense
        {
            public DateTimeOffset ExpirationDate { get; set; }
            public string InAppOfferToken { get; set; }
            public bool IsActive { get; set; }
            public string StoreId { get; set; }
        }

        public class StoreAppLicense
        {
            public Dictionary<string, StoreLicense> AddOnLicenses { get; set; }
            public DateTimeOffset ExpirationDate { get; set; }
            public bool IsActive { get; set; }
            public bool IsTrial { get; set; }
            public string StoreId { get; set; }
            public TimeSpan TrialTimeRemaining { get; set; }
            public string TrialUniqueId { get; set; }
        }

        #endregion
    }
}
