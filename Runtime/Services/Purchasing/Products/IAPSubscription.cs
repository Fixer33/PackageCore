using System;
using System.Globalization;
using UnityEngine;

namespace Core.Services.Purchasing.Products
{
    [CreateAssetMenu(fileName = "Subscription", menuName = "Services/Purchasing/Products/Subscription data", order = 0)]
    public class IAPSubscription : IAPProductBase
    {
        [SerializeField] private int _perMonthDivider = 1;

        private void OnValidate()
        {
            _perMonthDivider = Mathf.Max(1, _perMonthDivider);
        }

        public string GetPricePerMonth()
        {
            string fullPrice = GetPrice();
            if (string.IsNullOrEmpty(fullPrice)) return fullPrice;

            int firstDigitIndex = -1;
            int lastDigitIndex = -1;

            for (int i = 0; i < fullPrice.Length; i++)
            {
                if (char.IsDigit(fullPrice[i]))
                {
                    if (firstDigitIndex == -1) firstDigitIndex = i;
                    lastDigitIndex = i;
                }
            }

            if (firstDigitIndex == -1) return fullPrice;

            string prefix = fullPrice.Substring(0, firstDigitIndex);
            string suffix = fullPrice.Substring(lastDigitIndex + 1);
            string numberPart = fullPrice.Substring(firstDigitIndex, lastDigitIndex - firstDigitIndex + 1);

            char separator = '.';
            bool separatorFound = false;
            for (int i = numberPart.Length - 1; i >= 0; i--)
            {
                if (numberPart[i] == '.' || numberPart[i] == ',')
                {
                    separator = numberPart[i];
                    separatorFound = true;
                    break;
                }
            }

            string cleanNumber = "";
            bool decimalFound = false;
            for (int i = numberPart.Length - 1; i >= 0; i--)
            {
                char c = numberPart[i];
                if (char.IsDigit(c))
                {
                    cleanNumber = c + cleanNumber;
                }
                else if ((c == '.' || c == ',') && !decimalFound && separatorFound && c == separator)
                {
                    cleanNumber = '.' + cleanNumber;
                    decimalFound = true;
                }
            }

            if (!double.TryParse(cleanNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                return fullPrice;
            }

            double perMonth = value / Mathf.Max(1, _perMonthDivider);

            double truncatedValue = Math.Floor(perMonth * 100.0 + 1e-9) / 100.0;
            string formatted = truncatedValue.ToString("0.##", CultureInfo.InvariantCulture);
            if (separator == ',')
            {
                formatted = formatted.Replace('.', ',');
            }

            return prefix + formatted + suffix;
        }
    }
}