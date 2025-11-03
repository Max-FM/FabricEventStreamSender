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
            string eventHubName = GetRequiredSetting("eventHubName");
            string eventHubConnectionString = BuildEventHubConnectionString(eventHubName);

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

        private static string BuildEventHubConnectionString(string eventHubName)
        {
            var sharedAccessKeyName = GetRequiredSetting("sharedAccessKeyName");
            var primaryKey = config["primaryKey"];
            var secondaryKey = config["secondaryKey"];
            var connectionStringPrimary = config["connectionStringPrimary"];
            var connectionStringSecondary = config["connectionStringSecondary"];
            var useSecondaryKeySetting = config["useSecondaryKey"];

            bool useSecondaryKey = false;
            if (!string.IsNullOrWhiteSpace(useSecondaryKeySetting))
            {
                bool.TryParse(useSecondaryKeySetting, out useSecondaryKey);
            }

            var selectedKey = useSecondaryKey
                ? (!string.IsNullOrWhiteSpace(secondaryKey) ? secondaryKey : primaryKey)
                : (!string.IsNullOrWhiteSpace(primaryKey) ? primaryKey : secondaryKey);

            if (string.IsNullOrWhiteSpace(selectedKey))
            {
                throw new InvalidOperationException("At least one SAS key (primary or secondary) must be provided.");
            }

            var endpoint = ResolveEndpoint(connectionStringPrimary, connectionStringSecondary);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException("Unable to resolve Event Hubs endpoint. Provide connectionStringPrimary or connectionStringSecondary.");
            }

            return $"Endpoint=sb://{endpoint}/;SharedAccessKeyName={sharedAccessKeyName};SharedAccessKey={selectedKey};EntityPath={eventHubName}";
        }

        private static string ResolveEndpoint(string connectionStringPrimary, string connectionStringSecondary)
        {
            var source = !string.IsNullOrWhiteSpace(connectionStringPrimary)
                ? connectionStringPrimary
                : connectionStringSecondary;

            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var props = EventHubsConnectionStringProperties.Parse(source);
            return props.FullyQualifiedNamespace;
        }

        private static string GetRequiredSetting(string key)
        {
            var value = config[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Configuration setting '{key}' is required.");
            }

            return value;
        }
    }
}
