using System;
using System.Net;
using System.Net.Sockets;

namespace DotVVM.Framework.StartupPerfTests
{
    public static class NetworkingHelpers
    {
        public static int FindRandomPort()
        {
            int port;
            var random = new Random();
            do
            {
                port = 60000 + random.Next(5000);
            } while (!TestPort(port));

            return port;
        }

        private static bool TestPort(int port)
        {
            using (var client = new TcpClient())
            {
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Loopback, port));
                    client.Close();
                    return false;
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }
    }
}
