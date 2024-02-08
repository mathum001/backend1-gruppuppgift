using System;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Client!");
            StartClient();
        }

        static void StartClient()
        {
            try
            {
                // Skapa en TcpClient för att ansluta till servern
                TcpClient client = new TcpClient("127.0.0.1", 8080);
                Console.WriteLine("Connected to the server.");

                // Hämta NetworkStream för att skicka och ta emot data
                NetworkStream stream = client.GetStream();

                // Användaren väljer att registrera eller logga in
                while (true)
                {
                    Console.WriteLine("Enter 'register' or 'login':");
                    string userInput = Console.ReadLine();

                    if (userInput?.ToLower() == "register")
                    {
                        RegisterUser(stream);
                        break;
                    }
                    else if (userInput?.ToLower() == "login")
                    {
                        LoginUser(stream);
                        break;
                    }
                    else
                    {
                        System.Console.WriteLine("Error, wrong command");
                    }
                }

                // Skapa en ny tråd för att lyssna på meddelanden från servern
                Thread receiveThread = new Thread(() => ReceiveMessages(stream));
                receiveThread.Start();

                // Användaren väljer att skicka broadcast eller privat meddelande
                while (true)
                {
                    Console.WriteLine("Enter 'send' to broadcast or 'private' for a private message:");
                    string command = Console.ReadLine();

                    if (command?.ToLower() == "send")
                    {
                        BroadcastMessage(stream);
                    }
                    else if (command?.ToLower() == "private")
                    {
                        SendPrivateMessage(stream);
                    }
                    else
                    {
                        // Stäng klienten om ett ogiltigt kommando anges
                        client.Close();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // Visa felmeddelande om något går fel
                Console.WriteLine("Error: " + e.Message);
            }
        }
        // Funktion för att registrera en ny användare
        static void RegisterUser(NetworkStream stream)
        {
            // Be användaren ange användarnamn och lösenord
            Console.WriteLine("Enter username:");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();

            // Skicka registreringsdata till servern
            string dataToSend = $"register {username} {password}";
            SendData(stream, dataToSend);
        }

        // Funktion för att logga in en användare
        static void LoginUser(NetworkStream stream)
        {
            // Be användaren ange användarnamn och lösenord
            Console.WriteLine("Enter username:");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();

            // Skicka inloggningsdata till servern
            string dataToSend = $"login {username} {password}";
            SendData(stream, dataToSend);
        }

        // Funktion för att skicka ett broadcast-meddelande till servern
        static void BroadcastMessage(NetworkStream stream)
        {
            // Be användaren ange meddelandet
            Console.WriteLine("Enter your message:");
            string message = Console.ReadLine();

            // Skicka broadcast-meddelandet till servern
            string dataToSend = $"send {message}";
            SendData(stream, dataToSend);
        }
        //Funktion för att skicka privat-meddelande till en specifik användare
        static void SendPrivateMessage(NetworkStream stream)
        {
            // Be användaren ange mottagarens användarnamn
            Console.WriteLine("Enter recipient's username:");
            string recipient = Console.ReadLine();

            // Be användaren ange det privata meddelandet
            Console.WriteLine("Enter your private message:");
            string message = Console.ReadLine();

            // Skicka det privata meddelandet till servern
            string dataToSend = $"sendPrivate {recipient} {message}";
            SendData(stream, dataToSend);
        }

        static void SendData(NetworkStream stream, string data)
        {
            // Send the data to the server
            byte[] dataToSend = Encoding.ASCII.GetBytes(data);
            stream.Write(dataToSend, 0, dataToSend.Length);
        }

        static void ReceiveMessages(NetworkStream stream)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine(message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving message from server: " + e.Message);
            }
        }
    }
}

