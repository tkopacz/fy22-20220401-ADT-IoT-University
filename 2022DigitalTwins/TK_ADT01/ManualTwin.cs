using Azure;
using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TK_ADT01
{
    // Define a custom model type for the twin to be created

    internal class CustomDigitalTwin
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string Id { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public MyCustomDigitalTwinMetadata Metadata { get; set; } = new MyCustomDigitalTwinMetadata();

        [JsonPropertyName("Temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("Humidity")]
        public double Humidity { get; set; }
    }

    internal class CustomDigitalTwinWrong
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string Id { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public MyCustomDigitalTwinMetadata Metadata { get; set; } = new MyCustomDigitalTwinMetadata();

        [JsonPropertyName("Temp")]
        public double Temp { get; set; }

        [JsonPropertyName("Humidity")]
        public double Humidity { get; set; }
    }

    internal class MyCustomDigitalTwinMetadata
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.MetadataModel)]
        public string ModelId { get; set; }

        [JsonPropertyName("Temperature")]
        public DigitalTwinPropertyMetadata Temperature { get; set; }

        [JsonPropertyName("Humidity")] //
        public DigitalTwinPropertyMetadata Humidity { get; set; }
    }


    // Initialize properties and create the twin
    public class TwinOperationsCreateTwin
    {
        public async Task CreateTwinAsync(DigitalTwinsClient client)
        {
            // Initialize the twin properties
            var myTwin = new CustomDigitalTwin
            {
                Metadata = { ModelId = "dtmi:example:Room;1" },
                Temperature = 25.0,
                Humidity = 50.0,
            };

            // Create the twin
            try
            {
                string twinId = "abc123";
                Console.WriteLine($"CreateOrReplaceDigitalTwinAsync - {twinId}");
                Response<CustomDigitalTwin> response = await client.CreateOrReplaceDigitalTwinAsync(twinId, myTwin);
                Console.WriteLine($"Temperature last updated on {response.Value.Metadata.Temperature.LastUpdatedOn}");

                try
                {
                    Console.WriteLine("Error - Twin not fit");
                    CustomDigitalTwinWrong c = new CustomDigitalTwinWrong
                    {
                        Metadata = { ModelId = "dtmi:example:Room;1" },
                        Temp = 25.0,
                        Humidity = 50.0,
                    };
                    Response<CustomDigitalTwinWrong> r = await client.CreateOrReplaceDigitalTwinAsync("willnotbecreated", c);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("-----\r\nModel will not allow to create that twin\r\n");
                    Console.WriteLine(ex.ToString());
                }


                for (int i = 0; i < 10; i++)
                {
                    myTwin.Temperature += i;
                    twinId = $"id{i:000}";
                    Console.WriteLine($"CreateOrReplaceDigitalTwinAsync - {twinId}");
                    response = await client.CreateOrReplaceDigitalTwinAsync(twinId, myTwin);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
