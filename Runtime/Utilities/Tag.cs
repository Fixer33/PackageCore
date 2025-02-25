using System;
using UnityEngine;

namespace Core.Utilities
{
    [Serializable]
    public class Tag
    {
        public string Name => _name;
        
        [SerializeField] private string _name;

        public static implicit operator string(Tag tag) => tag._name;
    }
}