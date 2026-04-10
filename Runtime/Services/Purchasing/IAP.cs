#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using System;
using System.Threading;
using Core.Services.Purchasing.Products;
using Cysharp.Threading.Tasks;
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

        [Header("Initialization")]
        [SerializeField] private int _maxInitializationAttempts = 20;
        [SerializeField] private int _initializationReattemptIntervalMs = 1500;
        [Header("Debug")]
        [SerializeField] protected bool _isDebugPremium;
        [Header("Products")]
        [SerializeField] protected IAPProductBase[] _premiumProducts = Array.Empty<IAPProductBase>();
        [Space] private EventCallbackCollection _callbacks;
        private bool _isWaitingForInitialization;
        private CancellationTokenSource _initializationCts;

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
                InitializedCallback, InitializeFailedCallback, ProductPurchasedCallback, PurchaseFailedCallback, UnknownErrorCallback);

            _maxInitializationAttempts = Mathf.Max(1, _maxInitializationAttempts);
            _initializationCts = new();
            DoInitializationAttemptCycle(_maxInitializationAttempts, _initializationCts.Token).Forget();

            async UniTaskVoid DoInitializationAttemptCycle(int attempts, CancellationToken ct)
            {
                DateTime start = DateTime.Now;
                for (int i = 0; i < attempts; i++)
                {
                    _isWaitingForInitialization = true;
                    Debug.Log("Started IAP initialization");
                    Initialize(_callbacks);

                    await UniTask.WaitWhile(() => _isWaitingForInitialization, cancellationToken: ct);
                    
                    if (IsInitialized)
                        return;

                    int reattemptTime = _initializationReattemptIntervalMs * (i + 1) * (i + 1);
                    Debug.LogError($"Retrying to initialize IAP in {reattemptTime} milliseconds");
                    await UniTask.Delay(reattemptTime, cancellationToken: ct);
                }
                
                Debug.LogError("IAP initialization failed after " + attempts + " attempts and " + (DateTime.Now - start).TotalMilliseconds + " milliseconds");
            }
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
            
            _initializationCts?.Cancel();
            _initializationCts = null;
            
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
        
        protected void TriggerPremiumPurchasedEvent() => PremiumPurchased?.Invoke();

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
                    TriggerPremiumPurchasedEvent();
                    break;
                }
            }
        }

        private void InitializeFailedCallback(string errormessage)
        {
            Debug.LogError("IAP initialization failed. " + errormessage);
            InitializeFailed?.Invoke();
            
            _isWaitingForInitialization = false;
        }

        private void InitializedCallback()
        {
            Debug.Log("IAP initialized");
            IsServiceInitialized = true;
            IsInitialized = true;
            _isWaitingForInitialization = false;
            Initialized?.Invoke();
            
            _initializationCts?.Cancel();
            _initializationCts = null;
        }

        private void UnknownErrorCallback(string errormessage)
        {
            Debug.LogError("IAP has encountered an unknown error: " + errormessage);
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
            public delegate void ErrorMessageDelegate(string errorMessage);
            public delegate void PurchaseFailDelegate(IAPProductBase product, string errorMessage);

            public Action Initialized;
            public ErrorMessageDelegate InitializeFailed;
            public Action<IAPProductBase> ProductPurchased;
            public PurchaseFailDelegate ProductPurchaseFailed;

            public ErrorMessageDelegate UnknownErrorOccured;

            public EventCallbackCollection(Action initialized, ErrorMessageDelegate initializeFailed,
                Action<IAPProductBase> productPurchased, PurchaseFailDelegate purchaseFailed, 
                ErrorMessageDelegate unknownErrorOccured)
            {
                Initialized = initialized;
                InitializeFailed = initializeFailed;
                ProductPurchased = productPurchased;
                ProductPurchaseFailed = purchaseFailed;
                UnknownErrorOccured = unknownErrorOccured;
            }
        }
    }
}