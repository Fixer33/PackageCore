using System;
#if USE_UNITY_IAP && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
using System.Linq;
using Core.Services.DataSaving;
using Core.Services.Purchasing.Products;
using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#endif

namespace Core.Services.Purchasing
{
#if USE_UNITY_IAP && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
    [CreateAssetMenu(fileName = "Unity IAP", menuName = "Services/Purchasing/Unity IAP")]
    public class UnityIAP : IAP
    {
        public static Action<string> ValidationMethod = delegate {};
        
        [SerializeField] private IAPProductBase[] _products;
        [SerializeField, Tooltip("Will need Data saver instance")] private bool _saveNonConsumables;
        [SerializeField] private float _subscriptionStoreSyncTime = 10;
        private readonly Dictionary<string, Product> _productsCache = new(); 
        private readonly Dictionary<string, bool> _subscriptionsCache = new(); 
        private List<IAPProductBase> _allProducts;
        private StoreController _storeController;
        private EventCallbackCollection _callbacks;

        protected override async void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            if (_products is not { Length: > 0 } && _premiumProducts is not { Length: > 0 })
                throw new Exception("No products defined!");
            
            _callbacks = callbacksToInvoke;
            
            _storeController = UnityIAPServices.StoreController();
  
            _storeController.OnPurchasePending += OnPurchasePending;  
            _storeController.OnProductsFetched += OnProductsFetched;
            _storeController.OnPurchasesFetched += OnPurchasesFetched;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            _storeController.OnPurchaseFailed += OnPurchaseFailed;
            
            await _storeController.Connect();
  
            var initialProductsToFetch = new List<ProductDefinition>();
            
            _allProducts = new List<IAPProductBase>(_premiumProducts);
            for (int i = 0; i < _products.Length; i++)
            {
                if (_allProducts.Contains(_products[i]) == false)
                    _allProducts.Add(_products[i]);
            }
            foreach (var product in _allProducts)
            {
                initialProductsToFetch.Add(new ProductDefinition(product.GetId(), GetProductType(product)));
            }
  
            _storeController.FetchProducts(initialProductsToFetch);
            
            StoreSync(_subscriptionStoreSyncTime);
        }


        protected override void PurchaseProduct(IAPProductBase data)
        {
            if (_storeController != null)
            {
                _storeController.PurchaseProduct(data.GetId());
            }
            else
            {
                _callbacks.ProductPurchaseFailed?.Invoke(data, "Store controller not initialized");
            }
        }

        public override string GetProductCost(IAPProductBase product)
        {
            if (_storeController == null)
                return "";

            if (_productsCache.ContainsKey(product.GetId()) == false)
                return "price not found";
            
            var prod = _productsCache[product.GetId()];
            if (prod == null)
                return "0";
            
            return prod.metadata.localizedPriceString;
        }

        public override bool IsProductOwned(IAPProductBase data)
        {
            if (_storeController == null)
                return false;
        
            if (data is IAPSubscription sub)
            {
                return _subscriptionsCache.GetValueOrDefault(sub.GetId(), false);
            }
            else if (data is IAPConsumable)
            {
                return false;
            }
            else if (data is IAPNonConsumable nonConsumable)
            {
                if (_saveNonConsumables == false)
                    return false;
                return DataSaver.GetValueCustomKey(nonConsumable) == true;
            }

            return false;
        }

        public override void RestorePurchases()
        {
            _storeController.RestoreTransactions((restored, data) =>
            {
                if (restored)
                    Debug.Log("Purchases have been restored. " + data);
                else
                    Debug.Log("Failed to restore purchases. " + data);
            });
        }

        #region Callbacks

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription payload)
        {
            if (IsInitialized == false)
                _callbacks.InitializeFailed?.Invoke($"{payload.FailureReason}: {payload.Message}");
        }

        private void OnProductsFetchFailed(ProductFetchFailed payload)
        {
            if (IsInitialized)
                return;
            
            StringBuilder sb = new();
            payload.FailedFetchProducts.ForEach(i => sb.AppendLine(i.id));
            _callbacks.InitializeFailed?.Invoke($"{payload.FailureReason}. Products: {sb.ToString()}");
            
            sb.Clear();
        }

        private void OnPurchasePending(PendingOrder order)
        {
            bool validPurchase = true;
            string errorMsg = "";
            try
            {
                ValidationMethod?.Invoke(order.Info.Receipt);
            }
            catch (Exception e)
            {
                validPurchase = false;
                errorMsg = e.Message;

#if UNITY_EDITOR
                if (e is NotImplementedException)
                    validPurchase = true;
#endif
            }

            if (order.Info.PurchasedProductInfo.Count < 1)
            {
                _callbacks.UnknownErrorOccured?.Invoke("OnPurchasePending has received order with no purchased products");
                return;
            }

            if (order.Info.PurchasedProductInfo.Count > 1)
            {
                _callbacks.UnknownErrorOccured?.Invoke("OnPurchasePending has received order with more than 1 purchased product. \n" +
                                                       $"Only {order.Info.PurchasedProductInfo[0].productId} will be processed!");
            }

            IAPProductBase product = null;
            foreach (var productBase in _allProducts)
            {
                if (productBase.GetId().Equals(order.Info.PurchasedProductInfo[0].productId))
                {
                    product = productBase;
                    break;
                }
            }
            
            if (product == null)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, "No product found with id: " + order.Info.PurchasedProductInfo[0].productId);
                return;
            }
            
            if (validPurchase == false)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, errorMsg);
                return;
            }
            
#if UNITY_EDITOR
            bool isPurchased =
                UnityEditor.EditorUtility.DisplayDialog("Purchase confirm", "Is purchase successful?", "Success", "Cancel");     
            if (isPurchased == false)
            {
                _callbacks.ProductPurchaseFailed?.Invoke( product, "User cancelled");
                return;
            }
            _isDebugPremium = true;
#endif
            
            if (product is IAPNonConsumable nonConsumable && _saveNonConsumables)
            {
                DataSaver.SetValueCustomKey(nonConsumable, true);
            }

            if (product is IAPSubscription sub)
            {
                _subscriptionsCache[sub.GetId()] = true;
            }

            _callbacks.ProductPurchased?.Invoke(product);
            _storeController.ConfirmPurchase(order);
        }

        private void OnProductsFetched(List<Product> products)
        {
            foreach (var product in products)
            {
                string id = product.definition.id;
                if (_productsCache.TryAdd(id, product) == false)
                    _productsCache[id] = product;
            }

            _storeController.FetchPurchases();
            
            if (IsInitialized == false)
                _callbacks.Initialized?.Invoke();
        }

        private void OnPurchasesFetched(Orders orders)
        {
            // Process purchases, e.g. check for entitlements from completed orders

            foreach (var purchasedProductInfo in orders.ConfirmedOrders.SelectMany(i => i.Info.PurchasedProductInfo))
            {
                if (purchasedProductInfo.subscriptionInfo == null)
                    continue;

                bool isSubscribed = purchasedProductInfo.subscriptionInfo.IsSubscribed() == Result.True;
                if (_subscriptionsCache.ContainsKey(purchasedProductInfo.productId) == false)
                    _subscriptionsCache.Add(purchasedProductInfo.productId, isSubscribed);
                else
                    _subscriptionsCache[purchasedProductInfo.productId] |= isSubscribed;
            }
        }
        
        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            if (failedOrder.Info.PurchasedProductInfo.Count < 0)
            {
                _callbacks.UnknownErrorOccured?.Invoke("Purchase failed, but no products are in the order!");
                return;
            }

            List<IPurchasedProductInfo> productsInOrder = new(failedOrder.Info.PurchasedProductInfo);
            for (int i = 0; i < _products.Length; i++)
            {
                foreach (var productInfo in failedOrder.Info.PurchasedProductInfo)
                {
                    if (_products[i].GetId().Equals(productInfo.productId) == false)
                        continue;
                    
                    _callbacks.ProductPurchaseFailed?.Invoke(_products[i], failedOrder.FailureReason.ToString());
                    
                    if (productsInOrder.Contains(productInfo))
                        productsInOrder.Remove(productInfo);
                }
                
            }
            
            foreach (var purchasedProductInfo in productsInOrder)
            {
                Debug.LogWarning("Purchase failed, but no product found: " + purchasedProductInfo.productId + " : " + failedOrder.FailureReason.ToString());
            }
        }

        #endregion

        private ProductType GetProductType(IAPProductBase product) => product switch
        {
            IAPConsumable iapConsumable => ProductType.Consumable,
            IAPNonConsumable iapNonConsumable => ProductType.NonConsumable,
            IAPSubscription iapSubscription => ProductType.Subscription,
            _ => throw new ArgumentOutOfRangeException(nameof(product))
        };
        
        private async void StoreSync(float time)
        {
            while (Application.isPlaying && IsServiceInitialized)
            {
                _storeController.FetchPurchases();
                await Task.Delay(Convert.ToInt32(time * 1000));
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _subscriptionStoreSyncTime = Mathf.Clamp(_subscriptionStoreSyncTime, 30, 1000000);
        }
#endif
    }
#else
    public class UnityIAP : ServiceScriptableObject
    {
        public static Action<string> ValidationMethod = delegate {};

        public override void InitializeService()
        {
            throw new System.NotImplementedException();
        }
    }
#endif
}