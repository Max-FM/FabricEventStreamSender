
# Azure Event Hub Sender Sample Application

This is a .NET 8.0 C# console application that simulates IoT device readings and sends them to an Azure Event Hub-compatible endpoint (such as Microsoft Fabric EventStream).

## Features
- Continuously generates and sends random device readings as JSON messages.
- Simulates normal and anomalous readings.
- Uses configuration file for connection strings and message latency.
- Uses the [Bogus](https://github.com/bchavez/Bogus) library for realistic data generation.

## Configuration
Copy `EHSender/configurations.template.json` to `EHSender/configurations.json` and fill in your Event Hub-compatible connection strings:

```
{
  "connectionStringPrimary": "<your primary connection string>",
  "connectionStringSecondary": "<your secondary connection string>",
  "useSecondaryConnection": "false",
  "latencyMS": "1000" // Optional: delay in ms between messages
}
```

## Usage
1. Build the project:
	- `dotnet build EHSender/EHSender.csproj`
2. Run the application:
	- `dotnet run --project EHSender`
3. The app will continuously send messages until you stop it (Ctrl+C).

## Data Schema
Each message contains the following fields:

- `deviceID`: Randomly selected from a hard-coded list
- `readingDate`: Current UTC timestamp
- `_partitionKey`: Combination of deviceID and reading date's day (not used for partitioning)
- `readingLatitude`: Random decimal between -90 and 90
- `readingLongitude`: Random decimal between -180 and 180
- `readingPressure`: Calculated value, sometimes with anomaly
- `readingLevel`: Calculated value, sometimes with anomaly
- `deviceStatus`: Random string (e.g., "A99" for anomaly, or random pattern)

## Notes
- The application will use the secondary connection string if `useSecondaryConnection` is set to `true` and the value is provided.
- Message sending rate can be controlled with the `latencyMS` setting.
- The app prints the number of messages sent in the console.

---
For more details, see the code in `EHSender/Program.cs` and `EHSender/DataGenerator.cs`.
