using System;
using System.Net.Sockets;
using System.Text;


namespace client;

class Program
{
    static void StartClient()
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 8080);
                Console.WriteLine("Connected to the server.");

                NetworkStream stream = client.GetStream();

                Console.WriteLine("Enter 'register' or 'login':");
                string userInput = Console.ReadLine();

                if (userInput?.ToLower() == "register")
                {
                    RegisterUser(stream);
                }
                else if (userInput?.ToLower() == "login")
                {
                    LoginUser(stream);
                }

                //Ny tråd som lyssnar på medd från servern
                Thread receiveThread = new Thread(() => ReceiveMessages(stream));
                receiveThread.Start();

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
                        client.Close();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
}
