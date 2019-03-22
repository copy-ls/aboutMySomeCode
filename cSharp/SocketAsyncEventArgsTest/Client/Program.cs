using System;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("192.168.0.7", 1002);
            byte[] buffre = new byte[1];
            buffre[0] = 1;
            socket.Send(buffre);
        }
    }
}
