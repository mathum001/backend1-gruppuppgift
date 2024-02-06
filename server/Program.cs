using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.VisualBasic;
using MongoDB.Bson.Serialization.Attributes;
using BCrypt.Net;


using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.Common;
namespace server;

class Program
{
    static void Main(string[] args)
    {
        StartServer();
    }

    static void HandleClient(object klient)
    {
        TcpClient client = (TcpClient)klient;
        NetworkStream stream = client.GetStream();

        // Hantera kommunikationen med klienten
        try
        {
            while (client.Connected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // Om inga bytes läses in, betyder det att klienten har kopplat från
                    Console.WriteLine("Klienten har kopplat från.");
                    break;
                }
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Meddelande från klienten: " + dataReceived);

                // Hantera inkommande meddelanden och skicka svar
                string[] data = dataReceived.Split(" ");
                string command = data[0];
                string parameters = dataReceived.Substring(command.Length).Trim();


                if (commandActions.ContainsKey(command))
                {
                    commandActions[command].Invoke(parameters, stream);
                }
                else
                {
                    System.Console.WriteLine("Felaktigt kommando " + command);
                    SendMessage(" Ogiltigt kommando" + command, stream);
                }


                // Skicka tillbaka det mottagna meddelandet till klienten
                /* byte[] dataToSend = Encoding.ASCII.GetBytes(dataReceived);
                stream.Write(dataToSend, 0, dataToSend.Length); */
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error " + e);
        }
        finally
        {
            client.Close();
        }
    }
}
