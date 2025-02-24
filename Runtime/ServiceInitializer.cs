using System;
using System.Collections;
using System.Linq;
using Unity.Services.Core;
using UnityEngine;

namespace Core
{
    [AddComponentMenu("Custom/Initializer")]
    public class ServiceInitializer : MonoBehaviour
    {
        public static bool AreServicesInitialized { get; private set; }
        public static event Action ServicesInitialized;
        
        private ServiceBase[] _services;
        
        private async void Awake()
        {
#if USE_UNITY_SERVICES
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
#endif
            
            #if TEST
            Debug.Log(1);
            #endif
            
            _services = GetComponentsInChildren<ServiceBase>();
            for (int i = 0; i < _services.Length; i++)
            {
                try
                {
                    _services[i].InitializeService();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to init service {_services[i].GetType().Name}. Reason: {e.Message}");
                }
            }
            
            DontDestroyOnLoad(gameObject);

            StartCoroutine(InitializationWaiter());
        }

        private void OnDestroy()
        {
            AreServicesInitialized = false;
            ServicesInitialized = delegate { };
            for (int i = 0; i < _services.Length; i++)
            {
                try
                {
                    _services[i].DisposeService();
                }
                catch{}
            }
        }

        private IEnumerator InitializationWaiter()
        {
            yield return new WaitUntil(() => _services.All(i => i.IsServiceInitialized));
            AreServicesInitialized = true;
            ServicesInitialized?.Invoke();
        }
    }
}
