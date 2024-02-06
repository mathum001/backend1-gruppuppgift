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
}


class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

class Messages
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public List<string> UserMessages { get; set; }

    public Messages(int id, string userName, List<string> userMessages)
    {
        Id = id;
        UserName = userName;
        UserMessages = userMessages;
    }
}