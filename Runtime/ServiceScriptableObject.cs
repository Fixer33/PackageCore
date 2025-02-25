using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public abstract class ServiceScriptableObject : ScriptableObject, IService
    {
        public bool IsServiceInitialized { get; protected set; }
        
        public abstract void InitializeService();

        public virtual void DisposeService()
        {
            IsServiceInitialized = false;
        }
    }
}