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

    static string GetUsernameByStream(NetworkStream stream)
    {
        foreach (var kvp in userStreams)
        {
            if (kvp.Value == stream)
            {
                return kvp.Key; // Returnerar användarnamnet om nätverksströmmen matchar
            }
        }
        return null; // Returnerar null om ingen matchning hittades
    }
    
}
