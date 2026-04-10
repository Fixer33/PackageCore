#if USE_EXTENJECT
using Core.Utilities;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    [CreateAssetMenu(fileName = "Service installer", menuName = "Installers/Service", order = 0)]
    public class ServiceInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlatformDependentReference<ServiceScriptableObject[]> _services;
        [Header("Optional")]
        [Tooltip("Unity Services environment name. Leave empty to use default")]
        [SerializeField] private PlatformDependentReference<string> _unityServicesEnvironment;
        
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<ServiceInitializer>()
                .FromNew()
                .AsSingle()
                .WithArguments(_services.Get(), _unityServicesEnvironment.Get());
        }
    }
}
#else
#warning No extenject located! Please, import forked package via package-installer or set USE_EXTENJECT symbol after importing it other way
#endif