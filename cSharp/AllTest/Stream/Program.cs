using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Stream stream = new MemoryStream();
        byte[] writeBuffer = new byte[2];

        writeBuffer[0] = 1;
        writeBuffer[1] = 2;
        stream.Seek(2, SeekOrigin.Begin);
        stream.Write(writeBuffer, 0, 2);
        stream.Write(writeBuffer, 1, 1);
        byte[] readBuffer = new byte[stream.Length];
        int count = stream.Read(readBuffer, 0, 1);
    }
}
