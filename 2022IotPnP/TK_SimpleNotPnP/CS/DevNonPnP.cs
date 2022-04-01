using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK_SimplePnP
{
    internal class DevNonPnP
    {
        DeviceClient deviceClientNonPnP;
        ILogger logger;
        double temperature;
        public async Task Do()
        {
            temperature = Math.Round(Random.Shared.NextDouble() * 10.0 + 1.0, 1);
            string telemetryPayload = $"{{ \"abctemperaturka\": {temperature} }}";
            using var messagenonpnp = new Message(Encoding.UTF8.GetBytes(telemetryPayload))
            {
                ContentEncoding = "utf-8",
                ContentType = "application/json",
            };

            await deviceClientNonPnP.SendEventAsync(messagenonpnp);
            logger.LogDebug($"NonPnP - Telemetry: Sent - {temperature}°C.");
        }

        public DevNonPnP(ILogger log)
        {
            logger = log;
            deviceClientNonPnP = DeviceClient.CreateFromConnectionString(ConnectionStrings.ConnDevNonPnP, TransportType.Mqtt);
            deviceClientNonPnP.SetConnectionStatusChangesHandler((status, reason) =>
            {
                logger.LogDebug($"NonPnP - Connection status change registered - status={status}, reason={reason}.");
            });


        }
    }
}
