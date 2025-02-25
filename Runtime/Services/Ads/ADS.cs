#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using System;
using Core.Services.Purchasing;
using UnityEngine;

namespace Core.Services.Ads
{
    // ReSharper disable once InconsistentNaming
    public abstract class ADS : ServiceScriptableObject
    {
        public static event Action InterstitialShowStarted;
        public static event Action InterstitialHidden;
        
        private static ADS _instance;
        
        public delegate void FailAction(string errorMessage);

        [SerializeField] private bool _isBannerVisibleFromStart;
        [SerializeField, Tooltip("Do automatic checks for purchased premium")] private bool _iapConnectionEnabled = true;
        
        public override void InitializeService()
        {
            _instance = this;
            Initialize(new CallbackCollection()
            {
                Initialized = OnInitialized,
                InitializeFailed = OnInitFailed,
                
                InterstitialShowStarted = OnInterstitialShowStarted,
                InterstitialShown = OnInterstitialShown,
                InterstitialFailedToShow = OnInterstitialFailedToShow,
                
                RewardedSuccess = OnRewardedSuccess,
                RewardedFailed = OnRewardedFailed,
                
                AppOpenDismissed = OnAppOpenDismissed,
                AppOpenFailed = OnAppOpenFailed,
            });
            
            if (_iapConnectionEnabled == false)
                return;
            
            // Check for premium and automatically destroy inter and banner
            IAP.ExecuteOnInit(() =>
            {
                if (IAP.IsPremiumPurchased())
                {
                    DestroyBanner();
                    DestroyInterstitial();
                }
                else
                {
                    IAP.PremiumPurchased += IAPOnPremiumPurchased;
                }
            });
        }

        private void IAPOnPremiumPurchased()
        {
            IAP.PremiumPurchased -= IAPOnPremiumPurchased;
            DestroyBanner();
            DestroyInterstitial();
        }

        public override void DisposeService()
        {
            _instance = null;
            DisposeAds();
            base.DisposeService();
            
            IAP.PremiumPurchased -= IAPOnPremiumPurchased;
        }

        private void OnInitialized()
        {
            Debug.Log(GetType().Name + " initialized");

            if (_isBannerVisibleFromStart)
                SetBanner(true);
            
            IsServiceInitialized = true;
        }

        private void OnInitFailed(string error)
        {
            Debug.Log(GetType().Name + " failed to initialize: " + error);
            IsServiceInitialized = false;
        }

        protected abstract void Initialize(CallbackCollection callbackCollection);
        protected virtual void DisposeAds(){}
        protected abstract void ShowInterstitial();
        protected abstract void DestroyInterstitial();
        protected abstract void ShowRewarded();
        protected abstract void ShowAppOpen();
        protected abstract void SetBanner(bool isVisible);
        protected abstract void DestroyBanner();

        #region Interstitial

        private Action _onInterComplete, _onInterShown;
        private FailAction _onInterFailedToShow;
        
        private void OnInterstitialShown()
        {
            _onInterComplete?.Invoke();
            _onInterShown?.Invoke();
            
            _onInterComplete = null;
            _onInterShown = null;
            _onInterFailedToShow = null;
            
            InterstitialHidden?.Invoke();
        }
        
        private void OnInterstitialShowStarted()
        {
            InterstitialShowStarted?.Invoke();
        }

        private void OnInterstitialFailedToShow(string error)
        {
            _onInterComplete?.Invoke();
            _onInterFailedToShow?.Invoke(error);
            
            _onInterComplete = null;
            _onInterShown = null;
            _onInterFailedToShow = null;
            
            InterstitialHidden?.Invoke();
        }
        
        public static void ShowInterstitial(Action onComplete, Action onShowSuccess = null, FailAction onShowFail = null)
        {
            if (_instance == null)
            {
                onComplete?.Invoke();
                onShowFail?.Invoke("No ads instance initialized");
                return;
            }
            
            if (_instance._iapConnectionEnabled && IAP.IsPremiumPurchased())
            {
                onComplete?.Invoke();
                onShowFail?.Invoke("Won't show interstitial: User already has premium purchased!");
                return;
            }

            _instance._onInterComplete = onComplete;
            _instance._onInterShown = onShowSuccess;
            _instance._onInterFailedToShow = onShowFail;
            _instance.ShowInterstitial();
        }

        #endregion
        
        #region Rewarded

        private Action _onRewardedSuccess;
        private FailAction _onRewardedFailed;
        
        private void OnRewardedSuccess()
        {
            _onRewardedSuccess?.Invoke();
            
            _onRewardedSuccess = null;
            _onRewardedFailed = null;
        }

        private void OnRewardedFailed(string error)
        {
            _onRewardedFailed?.Invoke(error);
            
            _onRewardedSuccess = null;
            _onRewardedFailed = null;
        }
        
        public static void ShowRewarded(Action onShowSuccess, FailAction onShowFail)
        {
            if (_instance == null)
            {
                onShowFail?.Invoke("No ads instance initialized");
                return;
            }

            _instance._onRewardedSuccess = onShowSuccess;
            _instance._onRewardedFailed = onShowFail;
            _instance.ShowRewarded();
        }

        #endregion

        #region App Open

        private Action _onAppOpenDismissed;
        private FailAction _onAppOpenFailed;

        private void OnAppOpenDismissed()
        {
            _onAppOpenDismissed?.Invoke();
            
            _onAppOpenDismissed = null;
            _onAppOpenFailed = null;
        }

        private void OnAppOpenFailed(string error)
        {
            _onAppOpenFailed?.Invoke(error);
            
            _onAppOpenDismissed = null;
            _onAppOpenFailed = null;
        }
        
        public static void ShowAppOpen(Action onShowSuccess, FailAction onShowFail)
        {
            if (_instance == null)
            {
                onShowFail?.Invoke("No ads instance initialized");
                return;
            }

            _instance._onAppOpenDismissed = onShowSuccess;
            _instance._onAppOpenFailed = onShowFail;
            _instance.ShowAppOpen();
        }

        #endregion
        
        public static void SetBannerVisibility(bool isVisible)
        {
            if (_instance == null)
            {
                return;
            }

            _instance.SetBanner(isVisible);
        }

        public static async void ExecuteOnInit(Action action)
        {
            while (_instance == false || _instance.IsServiceInitialized == false)
            {
                if (Application.isPlaying == false)
                    return;
                
                await UnifiedTask.Delay(100);
            }
            
            action?.Invoke();
        }
        
        protected struct CallbackCollection
        {
            public Action Initialized;
            public FailAction InitializeFailed;

            public Action InterstitialShowStarted;
            public Action InterstitialShown;
            public FailAction InterstitialFailedToShow;

            public Action RewardedSuccess;
            public FailAction RewardedFailed;
            
            public Action AppOpenDismissed;
            public FailAction AppOpenFailed;
        }
    }
}