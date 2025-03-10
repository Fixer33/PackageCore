#if APPLOVIN
using System;
using System.Threading.Tasks;
#endif
using Core.Utilities;
using UnityEngine;

namespace Core.Services.Ads
{
    [CreateAssetMenu(fileName = "Applovin", menuName = "Services/Ads/Applovin", order = 0)]
    public class ApplovinAds : ADS
    {
        [SerializeField, TextArea] private string _sdkKey;
        [SerializeField] private PlatformDependentReference<string> _interstitialId;
        [SerializeField] private PlatformDependentReference<string> _rewardedId;
        [SerializeField] private PlatformDependentReference<string> _appOpenId;
        [SerializeField] private PlatformDependentReference<string> _bannerId;
#if APPLOVIN
        private InterstitialAd _interstitial;
        private RewardedAd _rewarded;
        private BannerAd _banner;
        private AppOpenAd _appOpen;
        private CallbackCollection _callbacks;

        protected override void Initialize(CallbackCollection callbackCollection)
        {
            _callbacks = callbackCollection;

            MaxSdkCallbacks.OnSdkInitializedEvent += MaxSdkCallbacksOnOnSdkInitializedEvent;
            MaxSdk.SetSdkKey(_sdkKey);
            MaxSdk.InitializeSdk();
        }

        protected override void DisposeAds()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent -= MaxSdkCallbacksOnOnSdkInitializedEvent;

            _interstitial.Destroy();
            _rewarded?.Destroy();
            _banner?.Destroy();
            _appOpen?.Destroy();
        }

        private void MaxSdkCallbacksOnOnSdkInitializedEvent(MaxSdkBase.SdkConfiguration config)
        {
            if (config.IsSuccessfullyInitialized == false)
            {
                _callbacks.InitializeFailed?.Invoke("Event came with initialization status of FALSE");
                return;
            }
            
            string adId = _interstitialId.Get();
            if (string.IsNullOrEmpty(adId) == false)
                _interstitial = new InterstitialAd(adId, () => _callbacks.InterstitialShowStarted?.Invoke());

            adId = _rewardedId.Get();
            if (string.IsNullOrEmpty(adId) == false)
                _rewarded = new RewardedAd(adId);
            
            adId = _bannerId.Get();
            if (string.IsNullOrEmpty(adId) == false)
                _banner = new BannerAd(adId, new Color(0, 0, 0, 0));
            
            adId = _appOpenId.Get();
            if (string.IsNullOrEmpty(adId) == false)
                _appOpen = new AppOpenAd(adId);
            
            _callbacks.Initialized?.Invoke();
        }

        protected override void ShowInterstitial()
        {
            if (_interstitial == null)
            {
                _callbacks.InterstitialFailedToShow?.Invoke("Interstitial ad was not created! Probably empty id");
                return;
            }
            
            _interstitial.Show(() => _callbacks.InterstitialShown?.Invoke(), 
                error => _callbacks.InterstitialFailedToShow?.Invoke(error));
        }

        protected override void DestroyInterstitial()
        {
            _interstitial = null;
        }

        protected override void ShowRewarded()
        {
            if (_rewarded == null)
            {
                _callbacks.RewardedFailed?.Invoke("Rewarded ad was not created! Probably empty id");
                return;
            }
            
            _rewarded.Show(() => _callbacks.RewardedSuccess?.Invoke(), 
                error => _callbacks.RewardedFailed?.Invoke(error));
        }

        protected override void ShowAppOpen()
        {
            if (_appOpen == null)
            {
                _callbacks.AppOpenFailed?.Invoke("App Open ad was not created! Probably empty id");
                return;
            }
            
            _appOpen.Show(() => _callbacks.AppOpenDismissed?.Invoke(), 
                error => _callbacks.AppOpenFailed?.Invoke(error));
        }

        protected override void SetBanner(bool isVisible)
        {
            _banner?.SetVisibility(isVisible);
        }

        protected override void DestroyBanner()
        {
            _banner?.Destroy();
            _banner = null;
        }

        #region Structures

        private class InterstitialAd
        {
            private readonly string _id;
            private readonly Action _showStarted;
            private Action _onComplete;
            private FailAction _onFailed;
            private int _retryAttempt;

            public InterstitialAd(string id, Action showStarted)
            {
                _id = id;
                _showStarted = showStarted;

                // Attach callback
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

                // Load the first interstitial
                LoadInterstitial();
            }

            public void Destroy()
            {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialLoadFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnInterstitialDisplayedEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialHiddenEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialAdFailedToDisplayEvent;
            }

            private void LoadInterstitial()
            {
                MaxSdk.LoadInterstitial(_id);
            }

            private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'

                // Reset retry attempt
                _retryAttempt = 0;
            }

            private async void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
            {
                // Interstitial ad failed to load
                // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

                _retryAttempt++;
                double retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));

                await Task.Delay(Convert.ToInt32(retryDelay * 1000));
                LoadInterstitial();
            }

            private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                _showStarted?.Invoke();
            }

            private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
                MaxSdkBase.AdInfo adInfo)
            {
                // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
                LoadInterstitial();
                _onFailed?.Invoke(errorInfo.Message);
            }

            private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
            }

            private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                // Interstitial ad is hidden. Pre-load the next ad.
                LoadInterstitial();
                _onComplete?.Invoke();
            }

            public void Show(Action onComplete = null, FailAction onFailed = null)
            {
                if (MaxSdk.IsInterstitialReady(_id) == false)
                {
                    onFailed?.Invoke("Interstitial is not yet ready to be shown");
                    return;
                }

                _onComplete = onComplete;
                _onFailed = onFailed;
                MaxSdk.ShowInterstitial(_id);
            }
        }

        private class RewardedAd
        {
            private readonly string _id;
            private Action _onComplete;
            private FailAction _onFail;
            private int _retryAttempt;

            public RewardedAd(string id)
            {
                _id = id;

                // Attach callback
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

                // Load the first rewarded ad
                LoadRewardedAd();
            }

            public void Destroy()
            {
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedAdClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardedAdRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdHiddenEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
            }

            private void LoadRewardedAd()
            {
                MaxSdk.LoadRewardedAd(_id);
            }

            private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

                // Reset retry attempt
                _retryAttempt = 0;
            }

            private async void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
            {
                // Rewarded ad failed to load
                // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

                _retryAttempt++;
                double retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));
                await Task.Delay(Convert.ToInt32(retryDelay * 1000));
                LoadRewardedAd();
            }

            private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
            }

            private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
                MaxSdkBase.AdInfo adInfo)
            {
                // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
                LoadRewardedAd();
                _onFail?.Invoke(errorInfo.Message);
            }

            private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
            }

            private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                // Rewarded ad is hidden. Pre-load the next ad
                LoadRewardedAd();
            }

            private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward,
                MaxSdkBase.AdInfo adInfo)
            {
                // The rewarded ad displayed and the user should receive the reward.
                _onComplete?.Invoke();
            }

            private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                // Ad revenue paid. Use this callback to track user revenue.
            }

            public void Show(Action onComplete, FailAction onFail)
            {
                if (MaxSdk.IsRewardedAdReady(_id) == false)
                {
                    _onFail?.Invoke("Rewarded is not ready yet");
                    return;
                }

                _onComplete = onComplete;
                _onFail = onFail;
                MaxSdk.ShowRewardedAd(_id);
            }
        }

        private class BannerAd
        {
            private readonly string _id;
            private bool _mustBeShowing;

            public BannerAd(string bannerAdUnitId, Color color)
            {
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
                MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
                MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;

                _id = bannerAdUnitId;
                // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
                // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
                MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

                // Set background or background color for banners to be fully functional
                MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, color);
            }

            public void Destroy()
            {
                MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnBannerAdLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent -= OnBannerAdClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnBannerAdRevenuePaidEvent;
                MaxSdkCallbacks.Banner.OnAdExpandedEvent -= OnBannerAdExpandedEvent;
                MaxSdkCallbacks.Banner.OnAdCollapsedEvent -= OnBannerAdCollapsedEvent;
                
                MaxSdk.DestroyBanner(_id);
            }

            public void SetVisibility(bool isVisible)
            {
                _mustBeShowing = isVisible;
                if (isVisible)
                {
                    MaxSdk.ShowBanner(_id);
                }
                else
                {
                    MaxSdk.HideBanner(_id);
                }
            }

            private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                Debug.Log($"Banner loaded at os: " + MaxSdk.GetBannerLayout(_id));
            }

            private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
            {
                Debug.LogError($"Failed to load banner! [{errorInfo.Code}] Msg:" + errorInfo.Message);
                if (_mustBeShowing)
                    MaxSdk.ShowBanner(_id);
            }

            private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
            }

            private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
            }

            private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
            }

            private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
            }
        }

        private class AppOpenAd
        {
            private readonly string _id;
            private Action _dismissed;
            private FailAction _failed;

            public AppOpenAd(string id)
            {
                _id = id;
                
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpenOnOnAdDisplayFailedEvent;
                MaxSdk.LoadAppOpenAd(_id);
            }

            public void Destroy()
            {
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent -= OnAppOpenDismissedEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent -= AppOpenOnOnAdDisplayFailedEvent;
            }

            private void AppOpenOnOnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo arg3)
            {
                _failed?.Invoke(errorInfo.Message);
            }

            public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
            {
                _dismissed?.Invoke();
                MaxSdk.LoadAppOpenAd(_id);
            }

            public void Show(Action dismissed, FailAction failed)
            {
                if (MaxSdk.IsAppOpenAdReady(_id) == false)
                {
                    failed?.Invoke("App open ad is not ready");
                    return;
                }

                _dismissed = dismissed;
                _failed = failed;
                MaxSdk.ShowAppOpenAd(_id);
            }
        }

        #endregion
        
#else
        protected override void Initialize(CallbackCollection callbackCollection)
        {
            callbackCollection.InitializeFailed?.Invoke("APPLOVIN symbols are not defined in project symbols");
        }

        protected override void DisposeAds() { }

        protected override void ShowInterstitial() {}

        protected override void DestroyInterstitial() { }

        protected override void ShowRewarded() { }

        protected override void ShowAppOpen() { }

        protected override void SetBanner(bool isVisible) { }

        protected override void DestroyBanner() { }
#endif
    }
}