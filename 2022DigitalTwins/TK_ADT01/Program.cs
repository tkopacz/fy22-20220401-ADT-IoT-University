using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Threading.Tasks;
using TK_ADT01;

Console.WriteLine("Hello, World!");

DigitalTwinsClient client;

try
{
    var credential = new DefaultAzureCredential(includeInteractiveCredentials: true); //Will re-use visual studio credentials or ask or ....
    //var credential = new AzureCliCredential();
    //var credential = new InteractiveBrowserCredential(tenantId: "72f988bf-86f1-41af-91ab-2d7cd011db47", clientId:"6a9b64b0-60bc-489f-a1f9-4cf0c899894e");
    client = new DigitalTwinsClient(new Uri("https://pltkw3adtdemo.api.weu.digitaltwins.azure.net"), credential);

    //Show delete/creation at the end!!!!
    //goto createtwinwithrelations;
    //goto querytwin;


    //Clean
    //return; //To avoid deletion by mistake 
    bool retry;
    Console.WriteLine("Clean");
    Task.Run(async () =>
    {
        do
        {
            retry = false;
            AsyncPageable<DigitalTwinsModelData> mdata = client.GetModelsAsync();
            await foreach (DigitalTwinsModelData md in mdata)
            {
                try
                {
                    Console.WriteLine($"DeleteModel - {md.Id}");
                    client.DeleteModel(md.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DeleteModel - error: " + ex.ToString());
                    retry = true;
                }
            }
        } while (retry);
    }).Wait();

    //Delete Twins
    Task.Run(async () =>
    {
        do
        {
            retry = false;
            AsyncPageable<BasicDigitalTwin> result = client.QueryAsync<BasicDigitalTwin>("Select * From DigitalTwins");
            await foreach (var twin in result)
            {
                try
                {
                    Console.WriteLine($"DeleteDigitalTwinAsync - {twin.Id}");
                    AsyncPageable<BasicRelationship> rels = client.GetRelationshipsAsync<BasicRelationship>(twin.Id);
                    await foreach (var r in rels)
                    {
                        await client.DeleteRelationshipAsync(r.SourceId, r.Id);
                    }
                    await client.DeleteDigitalTwinAsync(twin.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DeleteDigitalTwinAsync - error: " + ex.ToString());
                    retry = true;
                }
            }
        } while (retry);
    }).Wait();

    goto createtwinwithrelations;

    //Console.WriteLine("Enter - continue (create model - simple)"); Console.ReadLine();

    //1 - upload model
    var floor = File.ReadAllText(@"Z:\AzFY21-PaaS03\16-IoT-Hub\2022DigitalTwins\TK_ADT01\Model01VerySimple\Floor.json");
    var room = File.ReadAllText(@"Z:\AzFY21-PaaS03\16-IoT-Hub\2022DigitalTwins\TK_ADT01\Model01VerySimple\Room.json");
    client.CreateModels(new string[] { floor, room });

    //Parse Models
    ParseModelsSample pms = new ParseModelsSample();
    Task.Run(async () =>
    {
        await pms.ParseDemoAsync(client);
    }).Wait();

createtwin:
    //Console.WriteLine("Enter - continue (create twin)"); Console.ReadLine();
    TwinOperationsCreateTwin tct = new TwinOperationsCreateTwin();
    Task.Run(async () =>
    {
        await tct.CreateTwinAsync(client);
    }).Wait();

createtwinwithrelations:
    //ModelFirst
    //Console.WriteLine("Enter - continue (create twins & relations)"); Console.ReadLine();
    client.CreateModels(new string[] {
        File.ReadAllText(@"Z:\AzFY21-PaaS03\16-IoT-Hub\2022DigitalTwins\TK_ADT01\Model02Simple\deviceinformation-1.json"),
        File.ReadAllText(@"Z:\AzFY21-PaaS03\16-IoT-Hub\2022DigitalTwins\TK_ADT01\Model02Simple\m5go-1.json"),
        File.ReadAllText(@"Z:\AzFY21-PaaS03\16-IoT-Hub\2022DigitalTwins\TK_ADT01\Model02Simple\mym5go.json"),
        File.ReadAllText(@"Z:\AzFY21-PaaS03\16-IoT-Hub\2022DigitalTwins\TK_ADT01\Model02Simple\Room.json"),
        File.ReadAllText(@"Z:\AzFY21-PaaS03\16-IoT-Hub\2022DigitalTwins\TK_ADT01\Model02Simple\Floor.json"),
    });

    //Parse Models
    pms = new ParseModelsSample();
    Task.Run(async () =>
    {
        await pms.ParseDemoAsync(client);
    }).Wait();


    TwinRelationsCreateTwin tctr = new TwinRelationsCreateTwin();
    Task.Run(async () =>
    {
        await tctr.CreateTwinAsync(client);
        await tctr.CreateManyTwinAsync(client);
    }).Wait();

querytwin:
    var dt1 = (await client.GetDigitalTwinAsync<BasicDigitalTwin>("m5go0001")).Value; //From Event Hub etc.
    Console.WriteLine($"Model:{dt1.Metadata.ModelId}");
    Console.WriteLine($"Device information:{dt1.Contents["deviceInformation"]}");

    //What are the names of relations
    client.GetIncomingRelationshipsAsync("m5go0001");
    AsyncPageable<IncomingRelationship> rels = client.GetIncomingRelationshipsAsync("m5go0001");

    await foreach (IncomingRelationship ie in rels)
    {
        Console.WriteLine($"SourceId: {ie.SourceId} ,RelationshipId:{ie.RelationshipId}, RelationshipLink:{ie.RelationshipLink}, RelationshipName:{ie.RelationshipName}");
        //if (ie.RelationshipName == relname)
            //return (ie.SourceId);
    }

    var strquery = "SELECT Parent.$dtId FROM digitaltwins Parent JOIN Child RELATED Parent.contains WHERE Child.$dtId = 'm5go0001'";
    AsyncPageable<BasicDigitalTwin> twins = client.QueryAsync<BasicDigitalTwin>(strquery);
    string parentId="", grandparentId="";
    await foreach (BasicDigitalTwin twin in twins)
    {
        Console.WriteLine(twin.Id);
        parentId = twin.Id;
        break;
    }
    strquery = $"SELECT Parent.$dtId FROM digitaltwins Parent JOIN Child RELATED Parent.contains WHERE Child.$dtId = '{parentId}'";
    grandparentId = client.Query<BasicDigitalTwin>(strquery).AsEnumerable().FirstOrDefault().Id;
    Console.WriteLine($"m5go0001 - {parentId} - {grandparentId}");

    var updateTwinData = new JsonPatchDocument();
    var targettemp = 12 + Random.Shared.Next(10);
    updateTwinData.AppendReplace("/Temperature", targettemp);
    await client.UpdateDigitalTwinAsync(parentId, updateTwinData);
    updateTwinData = new JsonPatchDocument();
    updateTwinData.AppendAdd("/Temperature", targettemp);
    await client.UpdateDigitalTwinAsync(grandparentId, updateTwinData);
}
catch (Exception e)
{
    Console.WriteLine($"Authentication or client creation error: {e.Message}");
    Environment.Exit(0);
}


