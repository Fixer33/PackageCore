using System;
using System.Collections.Generic;
using Core.Utilities;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    [CreateAssetMenu(fileName = "Link installer", menuName = "Installers/Link", order = 0)]
    public class LinkInstaller : ScriptableObjectInstaller
    {
        private static Dictionary<LinkId, string> _links;
        
        [SerializeField] private PlatformDependentReference<string> _privacyPolicyMain;
        [SerializeField] private PlatformDependentReference<string> _termsOfUseMain;
        [SerializeField] private PlatformDependentReference<string> _storeLink;

        public override void InstallBindings()
        {
            _links = new();
            
            Container.Bind<string>().WithId(LinkId.TermsOfUse).FromInstance(_termsOfUseMain.Get());
            _links.Add(LinkId.TermsOfUse, _termsOfUseMain.Get());
            
            Container.Bind<string>().WithId(LinkId.PrivacyPolicy).FromInstance(_privacyPolicyMain.Get());
            _links.Add(LinkId.PrivacyPolicy, _privacyPolicyMain.Get());
            
            Container.Bind<string>().WithId(LinkId.StoreLink).FromInstance(_storeLink.Get());
            _links.Add(LinkId.StoreLink, _storeLink.Get());
        }

        protected virtual (LinkId id, string url)[] GetLinks() => Array.Empty<(LinkId id, string url)>();

        public static string GetLink(LinkId link)
        {
            if (_links == null || _links.ContainsKey(link) == false)
                return string.Empty;

            return _links[link];
        }
    }
}