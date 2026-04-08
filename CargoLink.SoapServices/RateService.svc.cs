using System.ServiceModel;
using CargoLink.SoapServices.DataContracts;

namespace CargoLink.SoapServices
{
    [ServiceContract(Namespace = "http://cargolink.com/rate")]
    public interface IRateService
    {
        [OperationContract]
        RateQuote[] GetRates(RateRequest request);

        [OperationContract]
        RateQuote GetRateForService(RateRequest request, string serviceLevel);
    }

    public class RateService : IRateService
    {
        public RateQuote[] GetRates(RateRequest request)
        {
            return new[]
            {
                new RateQuote
                {
                    ServiceLevel = "Ground",
                    BaseRate = request.PackageWeight * 0.50m,
                    FuelSurcharge = 5.00m,
                    TotalCost = (request.PackageWeight * 0.50m) + 5.00m,
                    EstimatedDays = 5
                },
                new RateQuote
                {
                    ServiceLevel = "Express",
                    BaseRate = request.PackageWeight * 1.20m,
                    FuelSurcharge = 8.00m,
                    TotalCost = (request.PackageWeight * 1.20m) + 8.00m,
                    EstimatedDays = 2
                },
                new RateQuote
                {
                    ServiceLevel = "Overnight",
                    BaseRate = request.PackageWeight * 2.50m,
                    FuelSurcharge = 12.00m,
                    TotalCost = (request.PackageWeight * 2.50m) + 12.00m,
                    EstimatedDays = 1
                }
            };
        }

        public RateQuote GetRateForService(RateRequest request, string serviceLevel)
        {
            var rates = GetRates(request);
            foreach (var rate in rates)
            {
                if (rate.ServiceLevel == serviceLevel)
                    return rate;
            }
            return null;
        }
    }
}
