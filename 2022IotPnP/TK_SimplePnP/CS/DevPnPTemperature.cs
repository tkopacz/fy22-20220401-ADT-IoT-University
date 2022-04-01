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
    internal class DevPnPTemperature
    {
        DeviceClient deviceClientThermostat;
        ILogger logger;
        double temperature = 0d;
        double maxTemp = 0d;
        public async Task Do()
        {
            temperature = Math.Round(Random.Shared.NextDouble() * 40.0 + 5.0, 1);
            string telemetryPayload = $"{{ \"temperature\": {temperature} }}";
            using var message = new Message(Encoding.UTF8.GetBytes(telemetryPayload)) { ContentEncoding = "utf-8", ContentType = "application/json", };

            await deviceClientThermostat.SendEventAsync(message);
            logger.LogDebug($"Thermostat - Telemetry: Sent - {temperature}°C }}.");
            if (temperature > maxTemp)
            {
                maxTemp = temperature;
                string propertyName = "maxTempSinceLastReboot";

                var reportedProperties = new TwinCollection();
                reportedProperties[propertyName] = maxTemp;

                await deviceClientThermostat.UpdateReportedPropertiesAsync(reportedProperties);
                logger.LogDebug($"Thermostat - Property: Update - {{ \"{propertyName}\": {maxTemp}°C }}");

            }
        }

        public DevPnPTemperature(ILogger log)
        {
            logger = log;
            var optionsThermostat = new ClientOptions
            {
                // DTDL interface used: https://github.com/Azure/iot-plugandplay-models/blob/main/dtmi/com/example/thermostat-1.json
                //https://docs.microsoft.com/en-us/azure/iot-develop/concepts-convention
                ModelId = "dtmi:com:example:Thermostat;1",
            };

            deviceClientThermostat = DeviceClient.CreateFromConnectionString(ConnectionStrings.ConnDevPnPTemperature, TransportType.Mqtt, optionsThermostat);
            deviceClientThermostat.SetConnectionStatusChangesHandler((status, reason) =>
            {
                logger.LogDebug($"Thermostat - Connection status change registered - status={status}, reason={reason}.");
            });
        }
    }
}
