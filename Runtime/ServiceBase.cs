using UnityEngine;

namespace Core
{
    public abstract class ServiceBase : MonoBehaviour
    {
        public abstract bool IsServiceInitialized { get; protected set; }
        
        public abstract void InitializeService();

        public virtual void DisposeService()
        {
            IsServiceInitialized = false;
        }
    }
}