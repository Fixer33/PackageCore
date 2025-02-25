using System;
using UnityEngine;

namespace Core.Utilities
{
    [Serializable]
    public abstract class TypeReference
    {
#if UNITY_EDITOR
        public virtual Type Editor_GetTypeBase() => null;
#endif
    }
    
    [Serializable]
    public class TypeReference<T> : TypeReference
    {
        [SerializeField] private string _typeName;
        private Type _type;
        private bool _noTypeFound;

#if UNITY_EDITOR
        public override Type Editor_GetTypeBase() => typeof(T);
#endif

        public Type Get()
        {
            if (_type != null || _noTypeFound)
                return _type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == null)
                    continue;
                
                _type = assembly.GetType(_typeName);
                if (_type != null)
                    return _type;
            }

            _noTypeFound = true;
            return null;
        }
    }
}