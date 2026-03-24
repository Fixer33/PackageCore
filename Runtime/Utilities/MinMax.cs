using System;
using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// Inclusive range of floats with helpers for random sampling and clamping.
    /// Ensures <see cref="Max"/> is never less than <see cref="Min"/>.
    /// </summary>
    [Serializable]
    public struct MinMax
    {
        /// <summary>
        /// Lower bound (inclusive).
        /// </summary>
        public float Min;
        /// <summary>
        /// Upper bound (inclusive).
        /// </summary>
        public float Max;

        /// <summary>
        /// Creates a new <see cref="MinMax"/>. If <paramref name="min"/> is greater than <paramref name="max"/>,
        /// the values are adjusted so that <see cref="Max"/> equals <paramref name="min"/>.
        /// </summary>
        public MinMax(float min, float max)
        {
            if (min > max)
                max = min;
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Validates and fixes the range if it becomes inverted.
        /// </summary>
        public void Validate()
        {
            if (Max < Min)
                Max = Min;
        }

        /// <summary>
        /// Returns a random value within the range.
        /// </summary>
        public float Random()
        {
            return UnityEngine.Random.Range(Min, Max);
        }

        /// <summary>
        /// Clamps the provided value to the range bounds.
        /// </summary>
        public float Clamp(float value)
        {
            return Mathf.Clamp(value, Min, Max);
        }
    }
}