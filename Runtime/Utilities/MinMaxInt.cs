using System;
using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// Inclusive range of integers with helpers for random sampling and clamping.
    /// Ensures <see cref="Max"/> is never less than <see cref="Min"/>.
    /// </summary>
    [Serializable]
    public struct MinMaxInt
    {
        /// <summary>
        /// Lower bound (inclusive).
        /// </summary>
        public int Min;
        /// <summary>
        /// Upper bound (inclusive).
        /// </summary>
        public int Max;

        /// <summary>
        /// Creates a new <see cref="MinMaxInt"/>. If <paramref name="min"/> is greater than <paramref name="max"/>,
        /// the values are adjusted so that <see cref="Max"/> equals <paramref name="min"/>.
        /// </summary>
        public MinMaxInt(int min, int max)
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
        /// Returns a random integer within the range.
        /// </summary>
        public int Random()
        {
            return UnityEngine.Random.Range(Min, Max);
        }

        /// <summary>
        /// Clamps the provided value to the range bounds.
        /// </summary>
        public int Clamp(int value)
        {
            return Mathf.Clamp(value, Min, Max);
        }
    }
}
