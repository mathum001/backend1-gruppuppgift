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

    static void SendMessage(string message, NetworkStream senderStream)
    {
        string username = GetUsernameByStream(senderStream);
        string messageToSend = $"{username + " skickade: " + message}";
        foreach (var kvp in userStreams)
        {
            if (kvp.Value != senderStream)
            {
                byte[] dataToSend = Encoding.ASCII.GetBytes(messageToSend);
                kvp.Value.Write(dataToSend, 0, dataToSend.Length);
                AddSingleMessageToDB(kvp.Value, message);
            }

        }
    }
    static void SendPrivateMessage(string parameters, NetworkStream senderStream)
    {
        string[] data = parameters.Split(" ");
        if (data.Length < 2)
        {
            Console.WriteLine("Incorrect private message format.");
            return;
        }
        string sender = GetUsernameByStream(senderStream);
        string recipient = data[0];
        string message = sender + ": " + parameters.Substring(recipient.Length).Trim();

        if (userStreams.TryGetValue(recipient, out NetworkStream recipientStream))
        {
            byte[] dataToSend = Encoding.ASCII.GetBytes(message);
            recipientStream.Write(dataToSend, 0, dataToSend.Length);
        }
        else
        {
            Console.WriteLine($"User '{recipient}' not found or offline.");
        }

        AddSingleMessageToDB(recipientStream, message);
    }
}
