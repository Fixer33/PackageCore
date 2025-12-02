using UnityEngine;
#if NETCODE_FOR_GAMEOBJECTS
using Unity.Netcode;
#endif

namespace Core.Utilities
{
    public class Singleton<T> where T : new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                instance = new T();
                return instance;
            }
        }
    }

    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Debug.LogError($"Second instance {gameObject.name} of class {typeof(T).Name} was destroed");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
        }
    }
    
#if NETCODE_FOR_GAMEOBJECTS
    public abstract class SingletonNetworkBehaviour<T> : NetworkBehaviour where T : SingletonNetworkBehaviour<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Debug.LogError($"Second instance {gameObject.name} of class {typeof(T).Name} was destroed");
                Destroy(gameObject);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }
    }
#endif
}