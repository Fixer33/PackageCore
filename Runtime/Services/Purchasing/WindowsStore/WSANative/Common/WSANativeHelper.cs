using System.Linq;

namespace CI.WSANative.Common
{
    public static class WSANativeHelper
    {
        /// <summary>
        /// Checks if the formatted price represents a zero value (i.e., contains no digits other than '0').
        /// </summary>
        /// <param name="formattedPrice">The formatted price string from the store.</param>
        /// <returns>True if the price is considered zero; otherwise, false.</returns>
        public static bool IsPriceZero(string formattedPrice)
        {
            if (string.IsNullOrEmpty(formattedPrice))
            {
                return true;
            }

            bool hasDigits = false;
            foreach (char c in formattedPrice)
            {
                if (char.IsDigit(c))
                {
                    hasDigits = true;
                    if (c != '0')
                    {
                        return false;
                    }
                }
            }

            // If there are no digits at all, we'll consider it zero or invalid.
            // If there are digits and none of them were non-zero, it's zero.
            return true;
        }
    }
}
