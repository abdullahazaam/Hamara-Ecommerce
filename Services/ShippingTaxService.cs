using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class ShippingTaxService : IShippingTaxService
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public ShippingTaxService(IConfiguration config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        private StoreSetting GetActiveSettings()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var setting = db?.StoreSettings.FirstOrDefault();
                if (setting != null) return setting;
            }
            catch
            {
                // Fallback to configuration / defaults if DbContext isn't available during migration or startup
            }

            return new StoreSetting
            {
                StoreName = _config["Commerce:StoreName"] ?? "Hamara Commerce",
                StoreEmail = _config["Commerce:StoreEmail"] ?? "support@hamaracommerce.pk",
                StorePhone = _config["Commerce:StorePhone"] ?? "+92 300 1234567",
                StoreAddress = _config["Commerce:StoreAddress"] ?? "Plaza 45, Main Boulevard, Gulberg III, Lahore, Pakistan",
                CurrencyCode = _config["Commerce:CurrencyCode"] ?? "PKR",
                CurrencySymbol = _config["Commerce:CurrencySymbol"] ?? "Rs. ",
                TaxRatePercent = decimal.TryParse(_config["Commerce:SalesTaxRate"], out var rate) ? rate * 100m : 5.0m,
                FreeShippingThreshold = decimal.TryParse(_config["Commerce:FreeShippingThreshold"], out var t) ? t : 5000.00m,
                StandardShippingFee = 250.0m,
                ExpressShippingFee = 500.0m,
                LowStockThreshold = 5,
                EnableGuestCheckout = true
            };
        }

        public string CurrencyCode => GetActiveSettings().CurrencyCode;
        public string CurrencySymbol => GetActiveSettings().CurrencySymbol;
        public decimal SalesTaxRate => GetActiveSettings().TaxRatePercent / 100m;
        public decimal FreeShippingThreshold => GetActiveSettings().FreeShippingThreshold;

        public List<ShippingMethodOption> GetAvailableShippingMethods(decimal subtotal, bool hasFreeShippingCoupon = false)
        {
            var s = GetActiveSettings();
            bool qualifiesForFree = subtotal >= s.FreeShippingThreshold || hasFreeShippingCoupon;

            return new List<ShippingMethodOption>
            {
                new ShippingMethodOption
                {
                    Code = "Standard",
                    Name = "Standard Courier (3-5 Days)",
                    Description = "Reliable nationwide door-to-door ground courier delivery.",
                    BaseCost = s.StandardShippingFee,
                    FreeOverSubtotal = s.FreeShippingThreshold,
                    CalculatedCost = qualifiesForFree ? 0.00m : s.StandardShippingFee,
                    FormattedCost = qualifiesForFree ? "FREE" : FormatCurrency(s.StandardShippingFee)
                },
                new ShippingMethodOption
                {
                    Code = "Express",
                    Name = "Express Courier (1-2 Days)",
                    Description = "Expedited air courier service with priority handling.",
                    BaseCost = s.ExpressShippingFee,
                    FreeOverSubtotal = 0m,
                    CalculatedCost = s.ExpressShippingFee,
                    FormattedCost = FormatCurrency(s.ExpressShippingFee)
                },
                new ShippingMethodOption
                {
                    Code = "Overnight",
                    Name = "Overnight VIP Courier",
                    Description = "Direct same-day/next-morning guaranteed delivery.",
                    BaseCost = 1000.00m,
                    FreeOverSubtotal = 0m,
                    CalculatedCost = 1000.00m,
                    FormattedCost = FormatCurrency(1000.00m)
                }
            };
        }

        public (bool isValid, decimal cost, string displayName) CalculateShippingFee(string methodCode, decimal subtotal, bool hasFreeShippingCoupon = false)
        {
            var options = GetAvailableShippingMethods(subtotal, hasFreeShippingCoupon);
            var selected = options.FirstOrDefault(o => string.Equals(o.Code, methodCode, StringComparison.OrdinalIgnoreCase));

            if (selected == null)
            {
                return (false, 0m, string.Empty);
            }

            return (true, selected.CalculatedCost, selected.Name);
        }

        public decimal CalculateTax(decimal taxableSubtotal)
        {
            if (taxableSubtotal <= 0) return 0m;
            return Math.Max(0m, Math.Round(taxableSubtotal * SalesTaxRate, 2));
        }

        public string FormatCurrency(decimal amount)
        {
            return $"{CurrencySymbol}{amount:N2}";
        }
    }
}
