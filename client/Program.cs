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

    static void RegisterUser(NetworkStream stream)
        {
            Console.WriteLine("Enter username:");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();

            // Send registration data to the server
            string dataToSend = $"register {username} {password}";
            SendData(stream, dataToSend);
        }
}
