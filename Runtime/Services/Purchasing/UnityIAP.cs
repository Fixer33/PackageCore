using System;
#if USE_UNITY_IAP && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
using System.Linq;
using Core.Services.DataSaving;
using Core.Services.Purchasing.Products;
using UnityEngine;
#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using System.Collections.Generic;
#endif

namespace Core.Services.Purchasing
{
#if USE_UNITY_IAP && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
    [CreateAssetMenu(fileName = "Unity IAP", menuName = "Services/Purchasing/Unity IAP")]
    public class UnityIAP : IAP, IDetailedStoreListener
    {
        public static Action<string> ValidationMethod = delegate {};
        
        [SerializeField] private IAPProductBase[] _products;
        [SerializeField, Tooltip("Will need Data saver instance")] private bool _saveNonConsumables;
        [SerializeField] private float _subscriptionStoreSyncTime = 10;
        private readonly Dictionary<IAPSubscription, bool> _subscriptionsCache = new(); 
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private EventCallbackCollection _callbacks;

        protected override void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            if (_products is not { Length: > 0 } && _premiumProducts is not { Length: > 0 })
                throw new Exception("No products defined!");
            
            _callbacks = callbacksToInvoke;
            
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            List<IAPProductBase> products = new List<IAPProductBase>(_premiumProducts);
            for (int i = 0; i < _products.Length; i++)
            {
                if (products.Contains(_products[i]) == false)
                    products.Add(_products[i]);
            }
            foreach (var product in products)
            {
                builder.AddProduct(product.GetId(), GetProductType(product));
            }
            
#if UNITY_EDITOR
            StandardPurchasingModule.Instance().useFakeStoreAlways = true;
#else
            StandardPurchasingModule.Instance().useFakeStoreAlways = false;
#endif
            
            UnityPurchasing.Initialize(this, builder);
        }

        protected override void PurchaseProduct(IAPProductBase data)
        {
            if (_storeController != null)
            {
                Product product = _storeController.products.WithID(data.GetId());

                if (product is { availableToPurchase: true })
                {
                    _storeController.InitiatePurchase(product);
                }
                else
                {
                    _callbacks.ProductPurchaseFailed?.Invoke(data, "Product not found or not available for purchase");
                }
            }
            else
            {
                _callbacks.ProductPurchaseFailed?.Invoke(data, "IAPManager not initialized");
            }
        }

        public override string GetProductCost(IAPProductBase product)
        {
            if (_storeController == null)
                return "";
            
            var prod = _storeController.products.WithID(product.GetId());
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
                return IsSubscribed(sub);
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
#if UNITY_STANDALONE_OSX || UNITY_IOS
            _extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((restored, data) =>
            {
                if (restored)
                    Debug.Log("Purchases have been restored. " + data);
                else
                    Debug.Log("Failed to restore purchases. " + data);
            });
#elif UNITY_WSA
            _extensionProvider.GetExtension<IMicrosoftExtensions>().RestoreTransactions();
#else
            Debug.LogWarning("Current platform either not supporting restoring or not implemented");
#endif
        }

        #region IAP Interface implementation

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _callbacks.InitializeFailed?.Invoke(nameof(error));
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            _callbacks.InitializeFailed?.Invoke($"{nameof(error)}: {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            bool validPurchase = true;
            string errorMsg = "";
            try
            {
                ValidationMethod?.Invoke(args.purchasedProduct.receipt);
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

            IAPProductBase product = null;
            foreach (var productBase in _products.Union(_premiumProducts))
            {
                if (GetProductType(productBase) == args.purchasedProduct.definition.type &&
                    productBase.GetId().Equals(args.purchasedProduct.definition.id))
                {
                    product = productBase;
                    break;
                }
            }
            
            if (product == null)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, "No product found with id: " + args.purchasedProduct.definition.id);
                return PurchaseProcessingResult.Complete;
            }
            
            if (validPurchase == false)
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, errorMsg);
                return PurchaseProcessingResult.Complete;
            }
            
#if UNITY_EDITOR
            bool isPurchased =
                UnityEditor.EditorUtility.DisplayDialog("Purchase confirm", "Is purchase successful?", "Success", "Cancel");     
            if (isPurchased == false)
            {
                _callbacks.ProductPurchaseFailed?.Invoke( product, "User cancelled");
                return PurchaseProcessingResult.Complete;
            }
            _isDebugPremium = true;
#endif
            
            if (product is IAPNonConsumable nonConsumable && _saveNonConsumables)
            {
                DataSaver.SetValueCustomKey(nonConsumable, true);
            }

            if (product is IAPSubscription sub)
            {
                _subscriptionsCache[sub] = true;
            }

            _callbacks.ProductPurchased?.Invoke(product);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            for (int i = 0; i < _products.Length; i++)
            {
                if (GetProductType(_products[i]) == product.definition.type && _products[i].GetId().Equals(product.definition.id))
                {
                    _callbacks.ProductPurchaseFailed?.Invoke(_products[i], nameof(failureReason));
                    return;
                }
            }
            
            
            Debug.LogWarning("Purchase failed, but no product found: " + product.definition.id + " : " + nameof(failureReason));
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            _callbacks.Initialized?.Invoke();

            StoreSync(_subscriptionStoreSyncTime);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            for (int i = 0; i < _products.Length; i++)
            {
                if (GetProductType(_products[i]) == product.definition.type && _products[i].GetId().Equals(product.definition.id))
                {
                    _callbacks.ProductPurchaseFailed?.Invoke(_products[i], failureDescription.message);
                    return;
                }
            }

            Debug.LogWarning("Purchase failed, but no product found: " + product.definition.id + " : " + failureDescription.message);
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
                SyncSubscriptionsWithStore();
                await UnifiedTask.Delay(Convert.ToInt32(time * 1000));
            }
        }

        private bool IsSubscribed(IAPSubscription subscription)
        {
            if (_subscriptionsCache.ContainsKey(subscription) == false)
                SyncSubscriptionsWithStore();

            return _subscriptionsCache[subscription];
        }

        private void SyncSubscriptionsWithStore()
        {
            foreach (var iapProductBase in _products.Union(_premiumProducts))
            {
                if (iapProductBase is not IAPSubscription sub)
                    continue;

                _subscriptionsCache[sub] = IsSubscribedInStore(sub);
            }
        }
        
        private bool IsSubscribedInStore(IAPSubscription subscription)
        {
            var prod = _storeController.products.WithID(subscription.GetId());
            if (prod is { receipt: not null })
            {
                SubscriptionManager sm = new SubscriptionManager(prod, null);
                try
                {
                    return sm.getSubscriptionInfo().isSubscribed() is Result.True;
                }
                catch
                {
                }
            }

            return false;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _subscriptionStoreSyncTime = Mathf.Clamp(_subscriptionStoreSyncTime, 0, 1000000);
            
            if (_saveNonConsumables && DataSaver.IsReady == false)
            {
                Debug.LogError("IAP is already instantiated but saver is not! Check if you have a DataSaver service or uncheck SaveNonConsumables!\n" +
                               "If you are in editor and have script - ignore this message");
            }
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