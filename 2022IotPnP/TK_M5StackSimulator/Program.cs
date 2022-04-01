// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using System.Text;
using TK_SimplePnP;

Console.WriteLine("Hello, World! Something will work soon");

//Logging

ILogger logger;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
    .AddFilter(level => level >= LogLevel.Debug)
    .AddSystemdConsole(options =>
    {
        options.TimestampFormat = "[yyyy/MM/dd HH:mm:ss]";
    });
});

logger = loggerFactory.CreateLogger("Main");
Console.WriteLine("Ctrl-C - break");
DevPnPM5Stack[] devarr = new DevPnPM5Stack[] {
    new DevPnPM5Stack(logger,ConnectionStrings.Connm5go0001),
    new DevPnPM5Stack(logger,ConnectionStrings.Connf0001r0000s0000),
    new DevPnPM5Stack(logger,ConnectionStrings.Connf0001r0003s0001),
};

Task.Run(async () => {
    do
    {
        for (int i = 0; i < devarr.Length; i++)
        {
            await devarr[i].Do();
        }
        global::System.Console.WriteLine(DateTime.Now.Ticks.ToString());
        await Task.Delay(10 * 1000); //ms
    } while (true);


}).Wait();

