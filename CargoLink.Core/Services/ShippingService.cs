using System;

namespace CargoLink.Core.Services
{
    public class ShippingService
    {
        public decimal CalculateShippingCost(decimal weight, string serviceLevel, string originZip, string destZip)
        {
            decimal baseRate = weight * GetRateMultiplier(serviceLevel);
            decimal fuelSurcharge = GetFuelSurcharge(serviceLevel);
            decimal distanceFactor = CalculateDistanceFactor(originZip, destZip);

            return (baseRate * distanceFactor) + fuelSurcharge;
        }

        private decimal GetRateMultiplier(string serviceLevel)
        {
            switch (serviceLevel?.ToLower())
            {
                case "ground": return 0.50m;
                case "express": return 1.20m;
                case "overnight": return 2.50m;
                default: return 0.50m;
            }
        }

        private decimal GetFuelSurcharge(string serviceLevel)
        {
            switch (serviceLevel?.ToLower())
            {
                case "ground": return 5.00m;
                case "express": return 8.00m;
                case "overnight": return 12.00m;
                default: return 5.00m;
            }
        }

        private decimal CalculateDistanceFactor(string originZip, string destZip)
        {
            return 1.2m;
        }

        public int GetEstimatedDays(string serviceLevel)
        {
            switch (serviceLevel?.ToLower())
            {
                case "ground": return 5;
                case "express": return 2;
                case "overnight": return 1;
                default: return 5;
            }
        }
    }
}
