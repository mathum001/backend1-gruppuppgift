using System;
using System.Net.Sockets;
using System.Text;


namespace client;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello Client!");
        StartClient();
    }

    static void SendData(NetworkStream stream, string data)
        {
            // Send the data to the server
            byte[] dataToSend = Encoding.ASCII.GetBytes(data);
            stream.Write(dataToSend, 0, dataToSend.Length);
        }
}
