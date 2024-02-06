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

    static void ReceiveMessages(NetworkStream stream)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine(message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving message from server: " + e.Message);
            }
  

    static void SendData(NetworkStream stream, string data)
        {
            // Send the data to the server
            byte[] dataToSend = Encoding.ASCII.GetBytes(data);
            stream.Write(dataToSend, 0, dataToSend.Length);
        }
}
