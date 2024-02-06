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
    static void RegisterUser(string parameters, NetworkStream stream)
    {
        System.Console.WriteLine("Du försökte göra en registrering" + parameters);
        string[] data = parameters.Split(" ");
        if (data.Length < 2)
        {
            Console.WriteLine("Felaktigt format på registrering.");
            return;
        }
        else
        {
            string userName = data[0];
            string password = data[1];
            Random random = new Random();
            int randomTal = random.Next(1, 1000);

            // Hash the user's password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            User newUser = new User
            {
                Id = randomTal,
                UserName = userName,
                Password = hashedPassword // Store hashed password in the database
            };

            IMongoCollection<User> users = FetchMongoUser();
            User existingUser = users.Find(x => x.UserName == userName).FirstOrDefault();


            if (existingUser != null)
            {
                System.Console.WriteLine("Användarnamnet är upptaget");
                return;
            }

            Add(users, newUser);

        }
    }
}
