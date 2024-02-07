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

    static void HandleClient(object klient)
    {
        TcpClient client = (TcpClient)klient;
        NetworkStream stream = client.GetStream();

        // Hantera kommunikationen med klienten
        try
        {
            while (client.Connected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // Om inga bytes läses in, betyder det att klienten har kopplat från
                    Console.WriteLine("Klienten har kopplat från.");
                    break;
                }
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Meddelande från klienten: " + dataReceived);

                // Hantera inkommande meddelanden och skicka svar
                string[] data = dataReceived.Split(" ");
                string command = data[0];
                string parameters = dataReceived.Substring(command.Length).Trim();


                if (commandActions.ContainsKey(command))
                {
                    commandActions[command].Invoke(parameters, stream);
                }
                else
                {
                    System.Console.WriteLine("Felaktigt kommando " + command);
                    SendMessage(" Ogiltigt kommando" + command, stream);
                }


                // Skicka tillbaka det mottagna meddelandet till klienten
                /* byte[] dataToSend = Encoding.ASCII.GetBytes(dataReceived);
                stream.Write(dataToSend, 0, dataToSend.Length); */
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error " + e);
        }
        finally
        {
            client.Close();
        }
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
