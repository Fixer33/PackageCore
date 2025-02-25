using UnityEngine;

namespace Utilities
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
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else
            {
                Debug.LogError($"Second instance {gameObject.name} of class {typeof(T).Name} was destroed");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }
    }
    
}