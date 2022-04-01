using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK_SimplePnP
{
    internal class DevPnPM5Stack
    {
        DeviceClient deviceClient;
        ILogger logger;
        double temperature = 0d;
        double maxTemp = 0d;
        public async Task Do()
        {
            var angle = Math.Round(Random.Shared.NextDouble() * 360);
            var pir = "false";
            double AccelX = Random.Shared.NextDouble(), AccelY = Random.Shared.NextDouble(), AccelZ = Random.Shared.NextDouble();
            double GyroX = Random.Shared.NextDouble(), GyroY = Random.Shared.NextDouble(), GyroZ = Random.Shared.NextDouble();
            double Temperature = 20 + 10.0 * Random.Shared.NextDouble();
            double Pressure = 1000 + 50 * Random.Shared.NextDouble();
            double Humidity = 10 + 90 * Random.Shared.NextDouble();

            string telemetryPayload = $"{{ \"angle\": {angle}, \"pir\":\"{pir}\", " +
                $"\"AccelX\":{AccelX}, \"AccelY\":{AccelY}, \"AccelZ\":{AccelZ}, " +
                $"\"GyroX\":{GyroX},\"GyroY\":{GyroY},\"GyroZ\":{GyroZ}, " +
                $"\"Temperature\":{Temperature},\"Pressure\":{Pressure},\"Humidity\":{Humidity} }}";

            using var message = new Message(Encoding.UTF8.GetBytes(telemetryPayload)) { ContentEncoding = "utf-8", ContentType = "application/json", };

            await deviceClient.SendEventAsync(message);
        }

        public DevPnPM5Stack(ILogger log, string connection)
        {
            logger = log;
            var options = new ClientOptions
            {
                // DTDL interface used: https://github.com/Azure/iot-plugandplay-models/blob/main/dtmi/com/example/thermostat-1.json
                //https://docs.microsoft.com/en-us/azure/iot-develop/concepts-convention
                ModelId = "dtmi:M5Stack:m5go;1",
            };

            deviceClient = DeviceClient.CreateFromConnectionString(connection, TransportType.Mqtt, options);
            deviceClient.SetConnectionStatusChangesHandler((status, reason) =>
            {
                logger.LogDebug($"Connection status change registered - status={status}, reason={reason}.");
            });
        }
    }
}
