using System;
using UnityEngine;

namespace Core.Utilities
{
    [Serializable]
    public struct MinMaxInt
    {
        public int Min;
        public int Max;

        public MinMaxInt(int min, int max)
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

        public int Random()
        {
            return UnityEngine.Random.Range(Min, Max);
        }
        
        public int Clamp(int value)
        {
            return Mathf.Clamp(value, Min, Max);
        }
    }
}
