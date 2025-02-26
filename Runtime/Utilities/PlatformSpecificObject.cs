using UnityEngine;

namespace Core.Utilities
{
    public class PlatformSpecificObject : MonoBehaviour
    {
        [SerializeField] private PlatformSpecification _platform;
        [SerializeField] private bool _disableInsteadOfDestroy;

        private void Awake()
        {
            if (PlatformUtility.IsForCurrentPlatform(_platform)) 
                return;
            
            if (_disableInsteadOfDestroy)
            {
                gameObject.SetActive(false);
                return;
            }

            Destroy(gameObject);
        }
    }
}
