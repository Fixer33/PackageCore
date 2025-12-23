#if SAMSUNG_BUILD
using System.Linq;
using Object = UnityEngine.Object;
#endif
using System;
using System.Collections.Generic;
using Core.Services.Purchasing.Products;
using UnityEngine;

namespace Core.Services.Purchasing.Samsung
{
    [CreateAssetMenu(fileName = "Samsung IAP", menuName = "Services/Purchasing/Samsung IAP")]
    public class SamsungIAP : IAP
    {
        private struct ProductData
        {
            public string Id;
            public string Price;
            public bool IsOwned;
        }
        
        [SerializeField] private bool _freeTrialAvailable;
        [SerializeField] private IAPProductBase[] _additionalProducts = Array.Empty<IAPProductBase>();
        private SamsungGameObject _samsungGameObject;
        private Dictionary<string, ProductData> _productsInfo;
        private AndroidJavaObject _iapInstance;
        private EventCallbackCollection _callbacks;
        private IAPProductBase _currentPurchasedItem;

#if SAMSUNG_BUILD
        public override string GetProductCost(IAPProductBase product)
        {
            if (_productsInfo.TryGetValue(product.GetId(), out var info))
                return info.Price;

            return $"No record of product {product.GetId()} found";
        }

        public override bool IsProductOwned(IAPProductBase data)
        {
            if (_productsInfo.TryGetValue(data.GetId(), out var info))
                return info.IsOwned;

            return false;
        }

        public override void RestorePurchases()
        {
        }

        protected override void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            _samsungGameObject = new GameObject("Samsung GO").AddComponent<SamsungGameObject>();
            Object.DontDestroyOnLoad(_samsungGameObject.gameObject);
            
            _callbacks = callbacksToInvoke;
            _productsInfo = new();
            
#if UNITY_EDITOR
            _productsInfo = _premiumProducts
                .Union(_additionalProducts)
                .ToDictionary(k => k.GetId(), v => new ProductData()
            {
                Id = v.GetId(),
                IsOwned = false,
                Price = "debug_price_0.1$"
            });
            _callbacks.Initialized?.Invoke();
            return;
#endif
            
            _samsungGameObject.Initialize(_iapInstance, initializeFailed: () =>
            {
                callbacksToInvoke.InitializeFailed?.Invoke("Failed to get instance of Samsung IAP java class!");
            });
            
            _samsungGameObject.GetOwnedList(SamsungGameObject.ItemType.all, productList =>
            {
                if (productList.results is { Count: > 0 })
                {
                    foreach (var item in productList.results)
                    {
                        _productsInfo.Add(item.mItemId, new ProductData()
                        {
                            Id = item.mItemId,
                            IsOwned = true,
                            Price = item.mItemPriceString
                        });
                    }
                }

                LoadNotOwnedProducts();
            });

            void LoadNotOwnedProducts()
            {
                string mergedIds = _premiumProducts
                    .Where(product => !_productsInfo.ContainsKey(product.GetId()))
                    .Aggregate("", (current, product) => current + (product.GetId() + ", "));
                mergedIds = _additionalProducts
                    .Where(product => !_productsInfo.ContainsKey(product.GetId()))
                    .Aggregate(mergedIds, (current, product) => current + (product.GetId() + ", "));

                if (string.IsNullOrEmpty(mergedIds) == false)
                    mergedIds = mergedIds.TrimEnd(',', ' ');
                
                Debug.Log(mergedIds);
                _samsungGameObject.GetProductsDetails(mergedIds, productList =>
                {
                    if (productList.results is not { Count: > 0 })
                    {
                        _callbacks.InitializeFailed?.Invoke(productList.errorInfo.errorString);
                        return;
                    }
                    
                    foreach (var item in productList.results)
                    {
                        _productsInfo.Add(item.mItemId, new ProductData()
                        {
                            Id = item.mItemId,
                            IsOwned = false,
                            Price = item.mItemPriceString
                        });
                    }
                    
                    _callbacks.Initialized?.Invoke();
                });
            }
        }

        protected override void PurchaseProduct(IAPProductBase product)
        {
#if UNITY_EDITOR
            bool isPurchased =
                UnityEditor.EditorUtility.DisplayDialog("Purchase confirm", "Is purchase successful?", "Success", "Cancel");     
            if (isPurchased)
            {
                _isDebugPremium = true;
                _callbacks.ProductPurchased?.Invoke(product);
            }
            else
            {
                _callbacks.ProductPurchaseFailed?.Invoke( product, "User cancelled");
            }
            // return;
#endif

            if (_currentPurchasedItem != null)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, "IAP already has a pending purchasing transaction!");
                return;
            }
            
            _currentPurchasedItem = product;
            
            _samsungGameObject.StartPayment(product.GetId(), "pass_through_value_test", args =>
            {
                if (args.errorInfo != null && string.IsNullOrEmpty(args.errorInfo.errorString) == false)
                {
                    _callbacks.ProductPurchaseFailed?.Invoke(_currentPurchasedItem, args.errorInfo.errorString);
                    _currentPurchasedItem = null;
                    return;
                }
                
                try
                {
                    string id = args.results.mItemId;
                    var data = _productsInfo[id];
                    data.IsOwned = true;
                    _productsInfo[id] = data;
                    _callbacks.ProductPurchased?.Invoke(_currentPurchasedItem);
                }
                catch (Exception e)
                {
                    _callbacks.ProductPurchaseFailed?.Invoke(_currentPurchasedItem, e.Message);
                }
                _currentPurchasedItem = null;
            });
        }

#else
        public override string GetProductCost(IAPProductBase product)
        {
            throw new NotImplementedException();
        }

        public override bool IsProductOwned(IAPProductBase product)
        {
            throw new NotImplementedException();
        }

        public override void RestorePurchases()
        {
            throw new NotImplementedException();
        }

        protected override void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            throw new NotImplementedException();
        }

        protected override void PurchaseProduct(IAPProductBase product)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
