using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;

namespace EHSender
{
    class Program
    {
        private static IConfiguration config;

        public static async Task Main(string[] args)
        {
            initializeConfigurations();
            string eventHubConnectionString = BuildEventHubConnectionString();

            int latency_ms = 0;
            int.TryParse(config["latencyMS"], out latency_ms);

            await using var producerClient = new EventHubProducerClient(eventHubConnectionString);

            Console.WriteLine("Start sending to Fabric event stream endpoint using Event Hubs protocol.");
            Console.WriteLine("press CTRL+C to stop sending");
            Console.WriteLine();

            long counter = 0;
            while (true)
            {
                await sendEventHubMessage(producerClient);
                counter += 1;
                int curpos = Console.CursorTop;
                Console.SetCursorPosition(0, curpos);
                Console.Write("{0} messages sent", counter);

                if (latency_ms > 0)
                {
                    System.Threading.Thread.Sleep(latency_ms);
                }
            }
        }

        private static async Task sendEventHubMessage(EventHubProducerClient producerClient)
        {
            DataGenerator generator = new DataGenerator();
            var message = generator.generateDateJson();

            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
            if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message))))
            {
                throw new InvalidOperationException("Event is too large for the batch.");
            }

            await producerClient.SendAsync(eventBatch);
        }

        private static void initializeConfigurations()
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("configurations.json")
                .Build();
        }

        private static string BuildEventHubConnectionString()
        {
            var primaryConnectionString = config["connectionStringPrimary"];
            var secondaryConnectionString = config["connectionStringSecondary"];
            var useSecondarySetting = config["useSecondaryConnection"];

            bool useSecondary = false;
            if (!string.IsNullOrWhiteSpace(useSecondarySetting))
            {
                bool.TryParse(useSecondarySetting, out useSecondary);
            }

            var selectedConnection = useSecondary
                ? (!string.IsNullOrWhiteSpace(secondaryConnectionString) ? secondaryConnectionString : primaryConnectionString)
                : (!string.IsNullOrWhiteSpace(primaryConnectionString) ? primaryConnectionString : secondaryConnectionString);

            if (string.IsNullOrWhiteSpace(selectedConnection))
            {
                throw new InvalidOperationException("At least one connection string (primary or secondary) must be provided.");
            }

            return selectedConnection;
        }

    }
}
