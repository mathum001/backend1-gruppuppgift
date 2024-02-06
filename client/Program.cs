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

    static void SendPrivateMessage(NetworkStream stream)
        {
            Console.WriteLine("Enter recipient's username:");
            string recipient = Console.ReadLine();

            Console.WriteLine("Enter your private message:");
            string message = Console.ReadLine();

            // Send private message to the server
            string dataToSend = $"sendPrivate {recipient} {message}";
            SendData(stream, dataToSend);
        }
}
