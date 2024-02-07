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

    static void Add(IMongoCollection<User> collection, User user)
    {
        collection.InsertOne(user);
        System.Console.WriteLine("Användare registrerad!");
    }
    static string AddSingleMessageToDB(NetworkStream stream, string message)
    {

        string username = GetUsernameByStream(stream);
        const string newpass = "KokxLPCVbH0hKrp2";
        string connectionUri = "mongodb+srv://mattiashummer:" + newpass + "@cluster0.y5yh9uz.mongodb.net/?retryWrites=true&w=majority";

        var settings = MongoClientSettings.FromConnectionString(connectionUri);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        var client = new MongoClient(settings);
        try
        {
            var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        // anslut till databasen samt lägger till en
        var database = client.GetDatabase("testing");

        IMongoCollection<Messages> messageCollection = database.GetCollection<Messages>("messages");

        var filter = Builders<Messages>.Filter.Eq(message => message.UserName, username);
        var update = Builders<Messages>.Update.Push(message => message.UserMessages, message);

        messageCollection.UpdateOne(filter, update);



        return null;
    }
}
