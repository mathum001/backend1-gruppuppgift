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

    static int Authenticate(string userName, string passWord)
    {
        int id = 0;
        IMongoCollection<User> users = FetchMongoUser();
        User user = users.Find(x => x.UserName == userName).FirstOrDefault();

        if (user != null)
        {
            // Verify the entered password with the stored hashed password
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(passWord, user.Password);

            if (isPasswordCorrect)
            {
                id = user.Id;
            }
        }
        return id;
    }
}
