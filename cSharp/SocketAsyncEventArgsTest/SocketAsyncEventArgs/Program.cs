using System;
using System.Net;

namespace TestSocketAsyncEventArgs
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(10, 10);
            server.Init();
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.7"), 1002);
            server.Start(iPEndPoint);
        }
    }
}
