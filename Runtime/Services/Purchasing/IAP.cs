#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using System;
using Core.Services.Purchasing.Products;
using UnityEngine;

namespace Core.Services.Purchasing
{
    public abstract class IAP : ServiceScriptableObject
    {
        public static event Action Initialized;
        public static event Action InitializeFailed;
        public static event Action<IAPProductBase> ProductPurchased;
        public static event Action<IAPProductBase> ProductPurchaseFailed;
        public static event Action PremiumPurchased;
        public static event Action ProductPurchaseStarted;
        public static event Action ProductPurchaseEnded;
        private static Action _onPurchaseComplete, _onPurchaseFail;

        public static bool IsInitialized { get; protected set; }
        public static IAP Instance { get; protected set; }

        [SerializeField] private bool _retryInitializationOnFail;
        [SerializeField] protected IAPProductBase[] _premiumProducts = Array.Empty<IAPProductBase>();
        [SerializeField] protected bool _isDebugPremium;
        [Space] private EventCallbackCollection _callbacks;

        public override void InitializeService()
        {
            if (Instance != null)
            {
                throw new Exception($"Trying to initialize second instance of IAP!\n " +
                                    $"Already active: {Instance.name}\n " +
                                    $"Trying to init: {name}");
            }

            Instance = this;
            _callbacks = new EventCallbackCollection(
                InitializedCallback, InitializeFailedCallback, ProductPurchasedCallback, PurchaseFailedCallback);
            Initialize(_callbacks);
        }
        
        public override void DisposeService()
        {
            base.DisposeService();
            
            IsInitialized = false;
            Instance = null;
            Initialized = delegate { };
            InitializeFailed = delegate { };
            ProductPurchased = delegate { };
            PremiumPurchased = delegate { };
            ProductPurchaseStarted = delegate { };
            ProductPurchaseEnded = delegate { };
            DisposeIAP();
        }

        public abstract string GetProductCost(IAPProductBase product);
        public abstract bool IsProductOwned(IAPProductBase product);
        public abstract void RestorePurchases();

        protected abstract void Initialize(EventCallbackCollection callbacksToInvoke);

        protected virtual void DisposeIAP()
        {
        }

        protected abstract void PurchaseProduct(IAPProductBase product);

        private void PurchaseFailedCallback(IAPProductBase product, string errormessage)
        {
            Debug.LogError("Product purchase failed: " + product.GetId() + "\n" + errormessage);

            ProductPurchaseEnded?.Invoke();
            ProductPurchaseFailed?.Invoke(product);
            _onPurchaseFail?.Invoke();
        }

        private void ProductPurchasedCallback(IAPProductBase product)
        {
            if (product == null)
            {
                Debug.LogWarning("Purchased product is null!");
                return;
            }

            Debug.Log("Product purchased: " + product.GetId());

            if (product is IAPConsumable consumable)
            {
                consumable.Consume();
            }

            ProductPurchaseEnded?.Invoke();
            ProductPurchased?.Invoke(product);
            _onPurchaseComplete?.Invoke();

            for (int i = 0; i < _premiumProducts.Length; i++)
            {
                if (_premiumProducts[i].Equals(product))
                {
                    PremiumPurchased?.Invoke();
                    break;
                }
            }
        }

        private void InitializeFailedCallback(string errormessage)
        {
            Debug.LogError("IAP initialization failed. " + errormessage);
            if (_retryInitializationOnFail)
            {
                Debug.LogWarning("Retrying to initialize IAP");
                Initialize(_callbacks);
            }
        }

        private void InitializedCallback()
        {
            IsServiceInitialized = true;
            IsInitialized = true;
            Initialized?.Invoke();
        }

        #region Static methods

        public static void Purchase(IAPProductBase product, Action onComplete = null, Action onFail = null)
        {
            if (product == null)
            {
                throw new NullReferenceException("Purchased product can't be null!");
            }

            Validate();

            _onPurchaseComplete = onComplete;
            _onPurchaseFail = onFail;

            ProductPurchaseStarted?.Invoke();
            Instance.PurchaseProduct(product);
        }

        public static bool IsPremiumPurchased()
        {
            Validate();

#if UNITY_EDITOR
            if (Instance._isDebugPremium)
                return true;
#endif
            for (int i = 0; i < Instance._premiumProducts.Length; i++)
            {
                if (Instance._premiumProducts[i].IsOwned())
                    return true;
            }

            return false;
        }

        public static void RestorePurchase()
        {
            Validate();
            Instance.RestorePurchases();
        }

        public static async UnifiedTask WaitForInitialization()
        {
            while (IsInitialized == false)
            {
                await UnifiedTask.Delay(100);
                if (Application.isPlaying == false)
                    return;
            }
        }

        public static async void ExecuteOnInit(Action action)
        {
            while (IsInitialized == false)
            {
                await UnifiedTask.Delay(100);
                if (Application.isPlaying == false)
                    return;
            }

            action?.Invoke();
        }

        protected static void Validate()
        {
            if (Instance == false)
                throw new Exception("No IAP existing! Please ensure to have IAP prefab on Init scene!");

            if (IsInitialized == false)
                throw new Exception(
                    "IAP is not initialized yet! Please, use IsInitialized to check for initialization\n" +
                    "OR await function WaitForInitialization\n" +
                    "OR use method ExecuteOnInit");
        }

        #endregion



        protected struct EventCallbackCollection
        {
            public delegate void InitFailDelegate(string errorMessage);
            public delegate void PurchaseFailDelegate(IAPProductBase product, string errorMessage);

            public Action Initialized;
            public InitFailDelegate InitializeFailed;
            public Action<IAPProductBase> ProductPurchased;
            public PurchaseFailDelegate ProductPurchaseFailed;

            public EventCallbackCollection(Action initialized, InitFailDelegate initializeFailed,
                Action<IAPProductBase> productPurchased, PurchaseFailDelegate purchaseFailed)
            {
                Initialized = initialized;
                InitializeFailed = initializeFailed;
                ProductPurchased = productPurchased;
                ProductPurchaseFailed = purchaseFailed;
            }
        }
    }
}