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
    internal class DevPnPCustom
    {
        ILogger logger;
        DeviceClient deviceClientPnPCustom;
        double voltage = 10;
        double current = 20;
        double targetVoltage = 10;
        double resistance = 5;

        public async Task Do()
        {
            voltage = targetVoltage - 0.05 * targetVoltage + Random.Shared.NextDouble() * 0.1 * targetVoltage;
            current = voltage / resistance;
            current = current - 0.05 * current + Random.Shared.NextDouble() * 0.1 * current; //Well - this is PHYSIC ;)

            string telemetryPayload = $"{{ \"voltage\": {voltage}, \"current\":{current} }}";
            using var messagepnpcustom = new Message(Encoding.UTF8.GetBytes(telemetryPayload)) { ContentEncoding = "utf-8", ContentType = "application/json", };

            await deviceClientPnPCustom.SendEventAsync(messagepnpcustom);
            logger.LogDebug($"PnPCustom - Telemetry: Sent - {telemetryPayload }");
        }

        public async Task InitDesiredProp()
        {
            //Init properties - demo - WILL NOT WORK :)
            //var reportedProperties = new TwinCollection();
            //reportedProperties["targetVoltage"] = voltage;
            //reportedProperties["resistance"] = resistance;
            //await deviceClientPnPCustom.UpdateReportedPropertiesAsync(reportedProperties);

            //Twin t = await deviceClientPnPCustom.GetTwinAsync();
            //t.Properties.Desired["targetVoltage"] = voltage;
            //t.Properties.Desired["resistance"] = resistance;
            //deviceClientPnPCustom.NOUPDATE FOR DESIRED STATE, BUT!!!

            var iotHubRegistryRW = Microsoft.Azure.Devices.RegistryManager.CreateFromConnectionString(ConnectionStrings.ConnIoTRegistry);
            Twin t = await iotHubRegistryRW.GetTwinAsync("dev-pnp-custom");
            t.Properties.Desired["targetVoltage"] = voltage;
            t.Properties.Desired["resistance"] = resistance;
            await iotHubRegistryRW.UpdateTwinAsync("dev-pnp-custom", t, t.ETag);
            var q=iotHubRegistryRW.CreateQuery("select * from devices where is_defined(modelId)"); // = 'dtmi:com:example:Thermostat;1'
            //Demo queries
            var result = await q.GetNextAsTwinAsync();
            foreach (var item in result)
            {
                logger.LogDebug($"Query1: {item.DeviceId} - {item.ModelId}");
            }

            q = iotHubRegistryRW.CreateQuery("select * from devices where modelId='dtmi:com:example:Thermostat;1'");  
            //Demo queries
            result = await q.GetNextAsTwinAsync();
            foreach (var item in result)
            {
                logger.LogDebug($"Query2: {item.DeviceId} - {item.ModelId}");
            }
        }

        public DevPnPCustom(ILogger log)
        {
            logger = log;
            deviceClientPnPCustom = DeviceClient.CreateFromConnectionString(ConnectionStrings.ConnDevPnPCustom, TransportType.Mqtt,
                new ClientOptions()
                {
                    ModelId = "dtmi:tkopacz:com:example:Voltage;1"
                }

            );

            deviceClientPnPCustom.SetConnectionStatusChangesHandler((status, reason) =>
            {
                logger.LogDebug($"PnPCustom - Connection status change registered - status={status}, reason={reason}.");
            });

            //Do
            deviceClientPnPCustom.SetMethodHandlerAsync("Do",
                async (status, result) => {
                    logger.LogDebug($"Do: {status.DataAsJson}");
                    return new MethodResponse(new byte[0], 200);
                }
            , null);
            //Do1 (not in model!)
            deviceClientPnPCustom.SetMethodHandlerAsync("Do1",
                async (status, result) => {
                    logger.LogDebug($"Do1: {status.DataAsJson}");
                    return new MethodResponse(new byte[0], 200);
                }
            , null);

            deviceClientPnPCustom.SetDesiredPropertyUpdateCallbackAsync(async (desiredProperties, reason) => {
                var reportedProperties = new TwinCollection();

                Console.WriteLine("\tDesired properties requested:");
                Console.WriteLine($"\t{desiredProperties.ToJson()}");

                // For the purpose of this sample, we'll blindly accept all twin property write requests.
                foreach (KeyValuePair<string, object> desiredProperty in desiredProperties)
                {
                    Console.WriteLine($"Setting {desiredProperty.Key} to {desiredProperty.Value}.");
                    reportedProperties[desiredProperty.Key] = desiredProperty.Value;
                    switch (desiredProperty.Key)
                    {
                        case "targetVoltage":
                            targetVoltage = double.Parse(desiredProperty.Value.ToString());
                            break;
                        case "resistance":
                            resistance = double.Parse(desiredProperty.Value.ToString());
                            break;
                    }
                }

                Console.WriteLine("\tAlso setting current time as reported property");
                reportedProperties["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.UtcNow;

                await deviceClientPnPCustom.UpdateReportedPropertiesAsync(reportedProperties);

            }, null);



            
        }
    }
}
