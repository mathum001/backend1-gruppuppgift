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

    static void StartServer()
    {
        TcpListener server = null;
        try
        {
            // Ange IP-adressen och porten som servern ska lyssna på
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8080;

            // Skapa en TCP-listener på den angivna IP-adressen och porten
            server = new TcpListener(ipAddress, port);

            // Starta lyssnaren
            server.Start();
            Console.WriteLine("Servern är igång och lyssnar på port " + port);



            while (true)
            {
                // Vänta på en anslutning från en klient
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("En klient har anslutit.");

                //ny tråd för separata klienter
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
        finally
        {
            // Stäng servern
            server?.Stop();
        }
    }
}
