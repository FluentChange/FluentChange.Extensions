using System;

namespace FluentChange.Extensions.Common.Models
{
    public class CurrencyAmount
    {
        public required string Currency { get; set; }
        public decimal Amount { get; set; }


        public CurrencyAmount Mutliply(double multiplier)
        {
            return new CurrencyAmount { Currency = Currency, Amount = Amount * Convert.ToDecimal(multiplier) };
        }
        public CurrencyAmount Mutliply(decimal multiplier)
        {
            return new CurrencyAmount { Currency = Currency, Amount = Amount * multiplier };
        }

        public CurrencyAmount Divide(decimal divider)
        {
            return new CurrencyAmount { Currency = Currency, Amount = Amount / divider };
        }

        public CurrencyAmount ToUnitPrice(decimal quantity)
        {
            return new CurrencyAmount { Currency = Currency, Amount = Amount / quantity };
        }

        public CurrencyAmount Round()
        {
            return new CurrencyAmount { Currency = Currency, Amount = Math.Round(Amount, 2) };
        }

        public CurrencyAmount Clone()
        {
            return new CurrencyAmount { Currency = Currency, Amount = Amount };
        }      
    }
}
