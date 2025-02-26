using System;
using System.Collections.Generic;
using Core.Services.Purchasing;
using Core.Utilities;
using UI;
using UnityEngine;

namespace Core.Services.Ads
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "Services/Automatic ad tracking", order = 0)]
    public class AutomaticAdTrackingService : ServiceScriptableObject
    {
        private static readonly List<AutomaticAdRequest> _requestsToRegisterOnInit = new();
        private static AutomaticAdTrackingService _instance;
        private static IView _paywall;

        [SerializeField] private bool _runAdOnInitialize;
        [SerializeField] private AutomaticAdCallData[] _primarySequence;
        [SerializeField] private AutomaticAdCallData[] _loopedSequence;
        private DateTime _lastShown = DateTime.MinValue;
        private float _currentCooldown;
        private int _primarySequenceIndex, _loopedSequenceIndex;
        private bool _hasPremium;
        
        public override void InitializeService()
        {
            _instance = this;
            _primarySequenceIndex = -1;
            _loopedSequenceIndex = -1;
            SubscribeToEvents(_requestsToRegisterOnInit.ToArray());
            _requestsToRegisterOnInit.Clear();
            
            IAP.PremiumPurchased += IAPOnPremiumPurchased;
            IsServiceInitialized = true;
            
            IAP.ExecuteOnInit(() =>
            {
                _hasPremium = IAP.IsPremiumPurchased();
            });
            
            if (_runAdOnInitialize)
                ADS.ExecuteOnInit(CallAd);
        }

        public override void DisposeService()
        {
            _instance = null;
            _paywall = null;
            _requestsToRegisterOnInit.Clear();
            base.DisposeService();
            IAP.PremiumPurchased -= IAPOnPremiumPurchased;
        }

        private void IAPOnPremiumPurchased()
        {
            _hasPremium = true;
            IAP.PremiumPurchased -= IAPOnPremiumPurchased;
        }

        private void CallAd()
        {
            if (_hasPremium)
                return;
            
            if ((DateTime.UtcNow - _lastShown).TotalSeconds < _currentCooldown)
                return;

            if (++_primarySequenceIndex < _primarySequence.Length)
            {
                ShowAd(_primarySequence[_primarySequenceIndex], () =>
                {
                    _primarySequenceIndex--;
                    _lastShown = DateTime.MinValue;
                });
                return;
            }

            if (_loopedSequence is not {Length: > 0})
                return;
            
            _loopedSequenceIndex = (_loopedSequenceIndex + 1) % _loopedSequence.Length;
            ShowAd(_loopedSequence[_loopedSequenceIndex], () =>
            {
                _loopedSequenceIndex--;
                _lastShown = DateTime.MinValue;
            });
        }

        private void ShowAd(AutomaticAdCallData callData, Action onFail)
        {
            Action onShowComplete = () => { _lastShown = DateTime.UtcNow; };
            
            _currentCooldown = callData.CooldownAfterCall;
            
            onShowComplete.Invoke();
            
            switch (callData.AdType)
            {
                case AutomaticAdType.None:
                    return;
                case AutomaticAdType.Paywall:
                    if (_paywall.IsAlive() == false)
                    {
                        onFail?.Invoke();
                        return;
                    }
                    _paywall.Show();
                    break;
                case AutomaticAdType.Interstitial:
                    ADS.ShowInterstitial(null, onShowComplete, error => onFail?.Invoke());
                    break;
                case AutomaticAdType.AppOpen:
                    ADS.ShowAppOpen(onShowComplete, error => onFail?.Invoke());
                    break;
            }
        }

        public static void RegisterPaywall(IView paywall)
        {
            if (paywall.IsAlive() == false)
                return;

            _paywall = paywall;
            _paywall.VisibilityChanged += (sender, args) =>
            {
                if (_instance == false)
                    return;

                _instance._lastShown = DateTime.UtcNow;
            };
        }

        public static void RegisterInterstitialRequestEvent(AutomaticAdRequest request)
        {
            if (_instance == false)
            {
                _requestsToRegisterOnInit.Add(request);
                return;
            }
            
            SubscribeToEvents(request);
        }

        private static void SubscribeToEvents(params AutomaticAdRequest[] requests)
        {
            foreach (var request in requests)
            {
                if (request == null)
                    continue;

                request.Triggered += () =>
                {
                    if (_instance == false)
                    {
                        return;
                    }
                    _instance.CallAd();
                };
            }
        }

        [Serializable]
        public struct AutomaticAdCallData
        {
            public AutomaticAdType AdType;
            public float CooldownAfterCall;
        }

        public enum AutomaticAdType
        {
            None,
            Paywall,
            Interstitial,
            AppOpen,
        }
    }
    
    public class AutomaticAdRequest
    {
        public event Action Triggered;

        private readonly PlatformSpecification _platform;

        public AutomaticAdRequest() : this(PlatformSpecification.ALL)
        {
            
        }
            
        public AutomaticAdRequest(PlatformSpecification platforms)
        {
            _platform = platforms;

            AutomaticAdTrackingService.RegisterInterstitialRequestEvent(this);
        }

        [Obsolete("Use Invoke instead")]
        public void Trigger()
        {
            Invoke();
        }
        
        public void Invoke()
        {
            if (PlatformUtility.IsForCurrentPlatform(_platform) == false)
                return;
                
            Triggered?.Invoke();
        }
    }
}