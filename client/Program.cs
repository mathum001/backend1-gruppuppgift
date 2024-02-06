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

    static void BroadcastMessage(NetworkStream stream)
        {
            Console.WriteLine("Enter your message:");
            string message = Console.ReadLine();

            // Send broadcast message to the server
            string dataToSend = $"send {message}";
            SendData(stream, dataToSend);
        }
}
