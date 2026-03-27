using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace EHSender
{
    class DataGenerator
    {
        private const int Slope = 5;
        private const double AnomalyProbability = 0.10; // 10% of records
        private static readonly Random _random = new Random();

        public string generateDateJson()
        {
            DeviceReading data = generateData();
            string JsonData = JsonSerializer.Serialize<DeviceReading>(data);
            return JsonData;
        }

        public DeviceReading generateData()
        {
            string[] deviceIDs = {
                "LHJ1879", "FRZ7757", "KPB4226", "NOX3665", "HQF2700", "AYE9130", "ZCS2987", "NIJ2249"
                // keep the rest of your IDs here
            };

            bool isAnomaly = _random.NextDouble() < AnomalyProbability;

            var testDevices = new Faker<DeviceReading>()
                .RuleFor(d => d.deviceID, f => f.PickRandomParam<string>(deviceIDs))
                .RuleFor(d => d.readingDate, f => DateTime.UtcNow)
                .RuleFor(d => d._partitionKey, (f, d) => d.deviceID + "-" + d.readingDate.Day)
                .RuleFor(d => d.readingLatitude, f => f.Random.Decimal(-90, 90))
                .RuleFor(d => d.readingLongitude, f => f.Random.Decimal(-180, 180))
                // Pressure: normal 90-110, anomaly high 200-600, anomaly low 0-40
                .RuleFor(d => d.readingPressure, (f, d) =>
                {
                    if (isAnomaly)
                    {
                        // 50% chance high, 50% chance low anomaly
                        if (_random.NextDouble() < 0.5)
                        {
                            // High anomaly
                            return f.Random.Decimal(200, 600); // 200-600
                        }
                        else
                        {
                            // Low anomaly
                            return f.Random.Decimal(0, 40);
                        }
                    }
                    else
                    {
                        return f.Random.Decimal(90, 110);
                    }
                })
                // Level: normal 0-10, anomaly high 20-60
                .RuleFor(d => d.readingLevel, (f, d) =>
                {
                    decimal baseLevel = f.Random.Decimal(0, 10);
                    return isAnomaly
                        ? baseLevel + f.Random.Decimal(20, 50)
                        : baseLevel;
                })
                .RuleFor(d => d.deviceStatus, (f, d) => isAnomaly ? "A99" : f.Random.Replace("?##"));

            return testDevices.Generate();
        }
    }

    class DeviceReading
    {
        public string _partitionKey { get; set; }
        public string deviceID { get; set; }
        public DateTime readingDate { get; set; }
        public decimal readingLatitude { get; set; }
        public decimal readingLongitude { get; set; }
        public decimal readingPressure { get; set; }
        public decimal readingLevel { get; set; }
        public string deviceStatus { get; set; }
    }
}
