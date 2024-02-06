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
