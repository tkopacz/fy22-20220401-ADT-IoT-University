using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System.Linq;

namespace Company.Function
{
    public static class pltkw3funtoupdateadt
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string adtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        [FunctionName("pltkw3funtoupdateadt")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Extract the body from the request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody)) {
                log.LogInformation("Connectivity ASA Test");
                return new StatusCodeResult(204);
            } // 204, ASA connectivity check

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Reject if too large, as per the doc
            if (data.ToString().Length > 262144) {return new StatusCodeResult(413);} //HttpStatusCode.RequestEntityTooLarge

            log.LogInformation($"Message:{requestBody}");

            //Processing, simplification - JArrary
            var deviceId = (string)data[0]["dev1"];
            var dev1maxt = (double)data[0]["dev1maxt"];
            var dev2maxt = (double)data[0]["dev2maxt"];
            
            var tempout = (dev1maxt + dev2maxt ) * 2.1; //Simulation

            log.LogInformation($"dev:{deviceId}, alg: {tempout}");
            
            //Update DT
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(
                new Uri(adtServiceUrl), credentials, new DigitalTwinsClientOptions
                { Transport = new HttpClientTransport(httpClient) });
            log.LogInformation($"ADT service client connection created.");

            var strquery = $"SELECT Parent.$dtId FROM digitaltwins Parent JOIN Child RELATED Parent.contains WHERE Child.$dtId = '{deviceId}'";
            var parentId = client.Query<BasicDigitalTwin>(strquery).AsEnumerable().FirstOrDefault().Id;
            log.LogInformation($"{deviceId} - {parentId}");

            var updateTwinData = new JsonPatchDocument();
            //updateTwinData.AppendReplace("/Temperature", tempout);
            updateTwinData.AppendAdd("/Temperature", tempout);
            await client.UpdateDigitalTwinAsync(parentId, updateTwinData);


            //return new OkObjectResult(responseMessage);
            return new OkResult();
        }
    }
}
