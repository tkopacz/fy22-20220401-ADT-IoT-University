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
DevNonPnP devNonPnP = new DevNonPnP(logger);

Task.Run(async () => {
    do
    {
        await devNonPnP.Do();
        await Task.Delay(5000);
    } while (true);


}).Wait();

