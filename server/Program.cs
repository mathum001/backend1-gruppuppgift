﻿using MongoDB.Driver;
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

    //Dictionary för commands
    static Dictionary<string, Action<string, NetworkStream>> commandActions = new Dictionary<string, Action<string, NetworkStream>>()
                {
                    { "register", RegisterUser},
                    { "login", LoginUser},
                    { "send", SendMessage},
                    { "sendPrivate", SendPrivateMessage},
                };

    //Dictionary för att servern ska hålla koll på vilken användare som är vilken
    static Dictionary<string, NetworkStream> userStreams = new Dictionary<string, NetworkStream>();
    
}
