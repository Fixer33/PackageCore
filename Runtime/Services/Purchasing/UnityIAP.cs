using System;
#if USE_UNITY_IAP
using Core.Services.Purchasing.Products;
using UnityEngine;
#endif

namespace Core.Services.Purchasing
{
#if USE_UNITY_IAP
    [CreateAssetMenu(fileName = "Unity IAP", menuName = "Services/Purchasing/Unity IAP")]
    public partial class UnityIAP : IAP
    {
        public static Action<string> ValidationMethod = delegate {};
        
        [SerializeField] private IAPProductBase[] _products;
        [SerializeField, Tooltip("Will need Data saver instance")] private bool _saveNonConsumables;
        [SerializeField] private float _subscriptionStoreSyncTime = 10;
        private EventCallbackCollection _callbacks;
        
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