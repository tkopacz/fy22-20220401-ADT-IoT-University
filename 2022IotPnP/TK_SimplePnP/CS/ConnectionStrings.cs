using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK_SimplePnP
{
    internal static class ConnectionStrings
    {
        public static string ConnDevNonPnP = "HostName=pltkw3adtiot.azure-devices.net;DeviceId=dev-nonpnp;SharedAccessKey=aaaaaaaaaaaaaaa=";
        public static string ConnDevPnPTemperature = "HostName=pltkw3adtiot.azure-devices.net;DeviceId=dev-pnp-temperature;SharedAccessKey=aaaaaaaaaaaa=";
        public static string ConnDevPnPCustom = "HostName=pltkw3adtiot.azure-devices.net;DeviceId=dev-pnp-custom;SharedAccessKey=aaaaaaaaaaaaaaaaaaaa=";
        public static string ConnIoTRegistry = "HostName=pltkw3adtiot.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=aaaaaaaaaaaaaaaaaaaa=";

    }
}
