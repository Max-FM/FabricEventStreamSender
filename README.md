# Azure Event Hub Sender Sample Application

Written in C# targeting .NET Core 8.0. The application simulates an IoT workload with device readings.

**Sample Data**
-
**deviceID**  Random selected from a list of device IDs hard-coded

**readingDate** Random recent date

**_partitionKey**  Combination of DeviceID and the reading date's year. *Not used as partition key for ingestion*

**readingLatitude** Random latitude with range from -90 to 90

**readingLogitude** Random longitude with range from -180 to 180

**readingPressure** Random value from 0 to 200

**level** Random value from 0 to 50

**deviceStatus** Random value of one character and two digits
