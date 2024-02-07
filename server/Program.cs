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
}
