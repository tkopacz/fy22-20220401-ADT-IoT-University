using Azure.DigitalTwins.Core;




internal class TwinRelationsCreateTwin
{
    public TwinRelationsCreateTwin()
    {
    }

    internal async Task CreateTwinAsync(DigitalTwinsClient client)
    {
        BasicDigitalTwin floor = new BasicDigitalTwin();
        BasicDigitalTwin room = new BasicDigitalTwin();
        floor.Metadata.ModelId = "dtmi:example:Floor;1";
        room.Metadata.ModelId = "dtmi:example:Room;1";
        floor.Id = "floor01";
        room.Id = "room01";
        await client.CreateOrReplaceDigitalTwinAsync(floor.Id, floor);
        await client.CreateOrReplaceDigitalTwinAsync(room.Id, room);

        BasicRelationship rroom = new BasicRelationship()
        {
            Id = "floor01room01",
            SourceId = "floor01",
            TargetId = "room01",
            Name = "contains",
            Properties =
                {
                    { "ownershipUser", "Someone" },
                    { "ownershipDepartment", "hr" }
                }
        };
        await client.CreateOrReplaceRelationshipAsync(rroom.SourceId, rroom.Id, rroom);


        BasicDigitalTwin m5go = new BasicDigitalTwin()
        {
            Metadata = { ModelId = "dtmi:example:M5Stack;1" }, //dtmi:M5Stack:m5go;1
            Id = "m5go0001",
            Contents =
                {
                    // digital twin properties
                    { "LightLeft", 12 },
                    { "LightRight", 13 },
                    // component
                    {
                        "deviceInformation",
                        new BasicDigitalTwinComponent
                        {
                            // component properties
                            Contents =
                            {
                                { "manufacturer", "Some china company" },
                                { "totalStorage", 2 },
                            },
                        }
                    },
                },


        };
        await client.CreateOrReplaceDigitalTwinAsync(m5go.Id, m5go);
        BasicRelationship rsenor = new BasicRelationship()
        {
            Id = "room01m5go0001",
            SourceId = "room01",
            TargetId = "m5go0001",
            Name = "contains",
            Properties =
                {
                    { "weight", 0.9 }
                }
        };
        await client.CreateOrReplaceRelationshipAsync(rsenor.SourceId, rsenor.Id, rsenor);

        BasicRelationship rsensortoroom = new BasicRelationship()
        {
            Id = "m5go0001room01",
            SourceId = "m5go0001",
            TargetId = "room01",
            Name = "inroom",
        };
        await client.CreateOrReplaceRelationshipAsync(rsensortoroom.SourceId, rsensortoroom.Id, rsensortoroom);

    }

    internal async Task CreateManyTwinAsync(DigitalTwinsClient client, int cntfloors = 3, int maxrooms = 3, int maxsenors = 2)
    {
        for (int ifloor = 0; ifloor < cntfloors; ifloor++)
        {
            Console.WriteLine($"Floor:{ifloor}");
            BasicDigitalTwin floor = new BasicDigitalTwin();
            floor.Metadata.ModelId = "dtmi:example:Floor;1";
            floor.Id = $"f{ifloor:0000}";
            await client.CreateOrReplaceDigitalTwinAsync(floor.Id, floor);

            int cntrooms = 5 + Random.Shared.Next(maxrooms);
            for (int iroom = 0; iroom < cntrooms; iroom++)
            {
                BasicDigitalTwin room = new BasicDigitalTwin();
                room.Metadata.ModelId = "dtmi:example:Room;1";
                room.Id = $"f{ifloor:0000}r{iroom:0000}";
                await client.CreateOrReplaceDigitalTwinAsync(room.Id, room);

                BasicRelationship rroom = new BasicRelationship()
                {
                    Id = $"relf{ifloor:0000}r{iroom:0000}",
                    SourceId = floor.Id,
                    TargetId = room.Id,
                    Name = "contains",
                    Properties =
                {
                    { "ownershipUser", $"Someone{iroom%10}" },
                    { "ownershipDepartment", $"hr{iroom%3}" }
                }
                };
                await client.CreateOrReplaceRelationshipAsync(rroom.SourceId, rroom.Id, rroom);

                int cntsensors = 1 + Random.Shared.Next(maxsenors - 1);
                for (int isensor = 0; isensor < cntsensors; isensor++)
                {
                    BasicDigitalTwin m5go = new BasicDigitalTwin()
                    {
                        Metadata = { ModelId = "dtmi:example:M5Stack;1" },
                        Id = $"f{ifloor:0000}r{iroom:0000}s{isensor:0000}",
                        Contents =
                        {
                            // digital twin properties
                            { "LightLeft", isensor+1},
                            { "LightRight", isensor+2 },
                            // component
                            {
                                "deviceInformation",
                                new BasicDigitalTwinComponent
                                {
                                    // component properties
                                    Contents =
                                    {
                                        { "manufacturer", "Some china company" },
                                        { "totalStorage", 2 + isensor },
                                    },
                                }
                            },
                        },


                    };
                    await client.CreateOrReplaceDigitalTwinAsync(m5go.Id, m5go);
                    BasicRelationship rsenor = new BasicRelationship()
                    {
                        Id = $"relf{ifloor:0000}r{iroom:0000}s{isensor:0000}",
                        SourceId = room.Id,
                        TargetId = m5go.Id,
                        Name = "contains",
                        Properties =
                        {
                            { "weight", 0.9 * Random.Shared.NextDouble() }
                        }
                    };
                    await client.CreateOrReplaceRelationshipAsync(rsenor.SourceId, rsenor.Id, rsenor);

                    if (isensor % 2 == 0)
                    {
                        BasicRelationship rsensortoroom = new BasicRelationship()
                        {
                            Id = $"rels{isensor:0000}r{iroom:0000}f{ifloor:0000}",
                            SourceId = m5go.Id,
                            TargetId = room.Id,
                            Name = "inroom",
                        };
                        await client.CreateOrReplaceRelationshipAsync(rsensortoroom.SourceId, rsensortoroom.Id, rsensortoroom);
                    }
                }
            }

        }



    }
}