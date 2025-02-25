#if USE_EXTENJECT
#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using System;
using System.Linq;
using Unity.Services.Core;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public class ServiceInitializer : IInitializable, IDisposable
    {
        public static bool AreServicesInitialized { get; private set; }
        public static event Action ServicesInitialized;

        private ServiceScriptableObject[] _services;

        public ServiceInitializer(ServiceScriptableObject[] services)
        {
            _services = services;
        }
        
        public async void Initialize()
        {
#if USE_UNITY_SERVICES
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
#endif
            
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

            while (Application.isPlaying && _services.Any(i => i.IsServiceInitialized == false))
            {
                await UnifiedTask.Delay(100);
            }
            
            AreServicesInitialized = true;
            ServicesInitialized?.Invoke();
        }

        public void Dispose()
        {
            AreServicesInitialized = false;
            ServicesInitialized = delegate { };
            for (int i = 0; i < _services.Length; i++)
            {
                if (_services[i].IsServiceInitialized == false)
                    continue;
                
                try
                {
                    _services[i].DisposeService();
                }
                catch{}
            }
        }
    }
}

#endif