using System;
using System.Collections.Generic;
using Core.Services.Purchasing;
using Core.Utilities;
using UI;
using UnityEngine;

namespace Core.Services.Ads
{
    [CreateAssetMenu(fileName = "Automatic ad tracking", menuName = "Services/Automatic ad tracking", order = 0)]
    public class AutomaticAdTrackingService : ServiceScriptableObject
    {
        private static readonly List<AutomaticAdRequest> _requestsToRegisterOnInit = new();
        private static readonly IList<IAdPaywall> _specialPaywalls = new List<IAdPaywall>();
        private static AutomaticAdTrackingService _instance;
        private static IAdPaywall _defaultPaywall;

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
            _defaultPaywall = null;
            _specialPaywalls.Clear();
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
                    IAdPaywall paywall = null;
                    foreach (var specialPaywall in _specialPaywalls)
                    {
                        if (specialPaywall.IsValidForSpecialShow)
                            paywall = specialPaywall;

                        if (paywall != null)
                            break;
                    }
                    paywall ??= _defaultPaywall;
                    
                    if (paywall.View.IsAlive() == false)
                    {
                        onFail?.Invoke();
                        return;
                    }
                    paywall.View.Show();
                    break;
                case AutomaticAdType.Interstitial:
                    ADS.ShowInterstitial(null, onShowComplete, error => onFail?.Invoke());
                    break;
                case AutomaticAdType.AppOpen:
                    ADS.ShowAppOpen(onShowComplete, error => onFail?.Invoke());
                    break;
            }
        }

        public static void RegisterPaywall(IAdPaywall paywall)
        {
            if (paywall == null || paywall.View.IsAlive() == false)
                return;

            switch (paywall.PaywallType)
            {
                case PaywallType.Default:
                    _defaultPaywall = paywall;
                    break;
                case PaywallType.Special:
                    _specialPaywalls.Add(paywall);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            paywall.View.VisibilityChanged += (sender, isVisible) =>
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
    
    public interface IAdPaywall
    {
        public PaywallType PaywallType { get; }
        public virtual bool IsValidForSpecialShow => true;
        
        public virtual IView View => this as IView; 
    }

    public enum PaywallType
    {
        Default,
        Special,
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
