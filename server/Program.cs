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

    static void LoginUser(string parameters, NetworkStream stream)
    {
        System.Console.WriteLine("Du försökte logga in" + parameters);
        string[] loginData = parameters.Split();
        if (loginData.Length != 2)
        {
            System.Console.WriteLine("Felaktiga uppgifter"); //TODO: skicka medd till client om att det ej gick
        }

        string userName = loginData[0];
        string passWord = loginData[1];
        int loginId = Authenticate(userName, passWord);

        if (loginId == 0)
        {
            System.Console.WriteLine("Felaktiga uppgifter");
        }
        else
        {
            userStreams[userName] = stream;
            string text = "Du loggades in!";
            System.Console.WriteLine("Inloggning lyckades: " + "-----" + userName);
            byte[] dataToSend = Encoding.ASCII.GetBytes(text);
            stream.Write(dataToSend, 0, dataToSend.Length);
            string loginMessage = userName + " har loggat in.";
            SendMessage(loginMessage, stream);

            Messages userMessages = FetchMongoMessages(userName);
            byte[] messageHistory = Encoding.ASCII.GetBytes("Meddelande Historik:");
            stream.Write(messageHistory, 0, messageHistory.Length);
            if (userMessages != null)
            {

                foreach (string message in userMessages.UserMessages)
                {
                    byte[] messageList = Encoding.ASCII.GetBytes(message + "\n");
                    stream.Write(messageList, 0, messageList.Length);

                }
            }

        }

    }
}
