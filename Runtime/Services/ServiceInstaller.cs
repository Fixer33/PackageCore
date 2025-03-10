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
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ServiceInitializer>().FromNew().AsSingle().WithArguments(_services.Get());
        }
    }
}
#else
#warning No extenject located! Please, import forked package via package-installer or set USE_EXTENJECT symbol after importing it other way
#endif