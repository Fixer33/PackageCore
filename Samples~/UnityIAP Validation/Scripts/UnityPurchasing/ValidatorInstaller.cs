using Core.Services.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing.Security;

namespace CrossPlatformValidation.Scripts.UnityPurchasing
{
    public static class ValidatorInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Install()
        {
            UnityIAP.ValidationMethod = receipt =>
            {
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(),
                    Application.identifier);
                var result = validator.Validate(receipt);
            };
        }
    }
}