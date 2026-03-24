using System;
using UnityEngine;

namespace Core.Utilities
{
    [Serializable]
    public struct MinMax
    {
        public float Min;
        public float Max;

        public MinMax(float min, float max)
        {
            if (min > max)
                max = min;
            Min = min;
            Max = max;
        }

        public void Validate()
        {
            if (Max < Min)
                Max = Min;
        }

        public float Random()
        {
            return UnityEngine.Random.Range(Min, Max);
        }
        
        public float Clamp(float value)
        {
            return Mathf.Clamp(value, Min, Max);
        }
    }
}