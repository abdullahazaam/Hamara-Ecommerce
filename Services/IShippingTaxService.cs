using System.Collections.Generic;

namespace HamaraCommerce.Services
{
    public class ShippingMethodOption
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public decimal FreeOverSubtotal { get; set; }
        public decimal CalculatedCost { get; set; }
        public string FormattedCost { get; set; } = string.Empty;
    }

    public interface IShippingTaxService
    {
        string CurrencyCode { get; }
        string CurrencySymbol { get; }
        decimal SalesTaxRate { get; }
        decimal FreeShippingThreshold { get; }

        List<ShippingMethodOption> GetAvailableShippingMethods(decimal subtotal, bool hasFreeShippingCoupon = false);
        (bool isValid, decimal cost, string displayName) CalculateShippingFee(string methodCode, decimal subtotal, bool hasFreeShippingCoupon = false);
        decimal CalculateTax(decimal taxableSubtotal);
        string FormatCurrency(decimal amount);
    }
}
