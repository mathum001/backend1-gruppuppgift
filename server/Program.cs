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

    static Messages FetchMongoMessages(string username)
    {
        const string newpass = "KokxLPCVbH0hKrp2";
        string connectionUri = "mongodb+srv://mattiashummer:" + newpass + "@cluster0.y5yh9uz.mongodb.net/?retryWrites=true&w=majority";

        var settings = MongoClientSettings.FromConnectionString(connectionUri);
        // Set the ServerApi field of the settings object to Stable API version 1
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        // Create a new client and connect to the server
        var client = new MongoClient(settings);
        // Send a ping to confirm a successful connection
        try
        {
            var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        // anslut till databasen
        var database = client.GetDatabase("testing");
        //anslut till kollektion
        IMongoCollection<Messages> collection = database.GetCollection<Messages>("messages");

        Messages existingMessages = collection.Find(x => x.UserName == username).FirstOrDefault();
        if (existingMessages == null)
        {
            Random random = new Random();
            int randomTal = random.Next(1, 1000);
            Messages tempMessage = new Messages(randomTal, username, new List<string>());
            collection.InsertOne(tempMessage);
            return tempMessage;
        }
        return existingMessages;
    }
    
}
