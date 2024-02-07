
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
using System.Threading;

namespace Server;

class Program
{
    static void Main(string[] args)
    {
        StartServer(); // Starta servern när programmet körs

    }


    static void StartServer()
    {
        TcpListener server = null;
        try
        {
            // Ange IP-adressen och porten som servern ska lyssna på
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8080;

            // Skapa en TCP-listener på den angivna IP-adressen och porten
            server = new TcpListener(ipAddress, port);

            // Starta lyssnaren
            server.Start();
            Console.WriteLine("Servern är igång och lyssnar på port " + port);



            while (true)
            {
                // Vänta på en anslutning från en klient
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("En klient har anslutit.");

                // Ny tråd för att hantera varje klient separat
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
        finally
        {
            // Stäng servern
            server?.Stop();
        }
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

                // Kolla om det finns en definierad åtgärd för det mottagna kommandot
                if (commandActions.ContainsKey(command))
                {
                    commandActions[command].Invoke(parameters, stream);
                }
                else
                {
                    System.Console.WriteLine("Felaktigt kommando " + command);
                    SendMessage(" Ogiltigt kommando" + command, stream);
                }
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

    static void RegisterUser(string parameters, NetworkStream stream)
    {
        // Skriv ut ett meddelande till konsolen för att indikera att registrering försöks
        System.Console.WriteLine("Du försökte göra en registrering" + parameters);
        
        // Dela upp parametrarna för registreringen (antagande av att användarnamn och lösenord är skilda av ett mellanslag)
        string[] data = parameters.Split(" ");

        // Kontrollera att det finns tillräckligt med parametrar för en registrering
        if (data.Length < 2)
        {
            Console.WriteLine("Felaktigt format på registrering.");
            return;
        }
        else
        {
            // Hämta användarnamn och lösenord från parametrarna
            string userName = data[0];
            string password = data[1];

            // Generera ett slumpmässigt tal för användarens ID
            Random random = new Random();
            int randomTal = random.Next(1, 1000);

            // Kryptera användarens lösenord med BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Skapa en ny användare med genererat ID, användarnamn och krypterat lösenord
            User newUser = new User
            {
                Id = randomTal,
                UserName = userName,
                Password = hashedPassword // Store hashed password in the database
            };

            // Hämta MongoDB-kollektionen för användare
            IMongoCollection<User> users = FetchMongoUser();
            User existingUser = users.Find(x => x.UserName == userName).FirstOrDefault();


            if (existingUser != null)
            {
                // Användarnamnet är upptaget, skriv ut meddelande och avbryt registreringen
                System.Console.WriteLine("Användarnamnet är upptaget");
                return;
            }

            // Lägg till den nya användaren i databasen
            Add(users, newUser);

        }
    }

    static void Add(IMongoCollection<User> collection, User user)
    {
        // Lägg till användaren i MongoDB-kollektionen
        collection.InsertOne(user);
        System.Console.WriteLine("Användare registrerad!");
    }

    static void LoginUser(string parameters, NetworkStream stream)
    {
        System.Console.WriteLine("Du försökte logga in" + parameters);

        // Dela upp inloggningsuppgifterna (antagande av att användarnamn och lösenord är skilda av ett mellanslag)
        string[] loginData = parameters.Split();

        // Kontrollera att det finns exakt två uppgifter för inloggning
        if (loginData.Length != 2)
        {
            System.Console.WriteLine("Felaktiga uppgifter"); //TODO: skicka medd till client om att det ej gick att logga in, vänligen försök igen. 
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
            // Lägg till användarens nätverksström i en dictionary för att hålla koll på anslutna användare
            userStreams[userName] = stream;

            // Skicka ett meddelande till klienten om att inloggningen lyckades
            string text = "Du loggades in!";
            System.Console.WriteLine("Inloggning lyckades: " + "-----" + userName);
            byte[] dataToSend = Encoding.ASCII.GetBytes(text);
            stream.Write(dataToSend, 0, dataToSend.Length);

            // Skicka ett meddelande till alla anslutna användare om den nya inloggningen
            string loginMessage = userName + " har loggat in.";
            SendMessage(loginMessage, stream);

            // Hämta meddelandehistorik för användaren och skicka den till klienten
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

    static int Authenticate(string userName, string passWord)
    {
        int id = 0;

        // Hämta MongoDB-kollektionen för användare
        IMongoCollection<User> users = FetchMongoUser();
        // Hämta användaren från databasen baserat på användarnamnet
        User user = users.Find(x => x.UserName == userName).FirstOrDefault();

        if (user != null)
        {
            // Verifiera det angivna lösenordet med det lagrade krypterade lösenordet
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(passWord, user.Password);

            if (isPasswordCorrect)
            {
                // Om lösenordet är korrekt, sätt ID:t till användarens ID
                id = user.Id;
            }
        }
        return id;
    }

    static void SendMessage(string message, NetworkStream senderStream)
    {
        // Hämta avsändarens användarnamn med hjälp av nätverksströmmen
        string username = GetUsernameByStream(senderStream);

         // Skapa meddelandet som ska skickas, inklusive användarnamn och själva meddelandet
        string messageToSend = $"{username + " skickade: " + message}";

        // Loopa igenom alla användarströmmar
        foreach (var kvp in userStreams)
        {
            // Kontrollera att det inte är samma ström som avsändaren
            if (kvp.Value != senderStream)
            {
                // Konvertera meddelandet till byte-array för att skicka över nätverket
                byte[] dataToSend = Encoding.ASCII.GetBytes(messageToSend);
                  // Skicka meddelandet till den aktuella användarströmmen
                kvp.Value.Write(dataToSend, 0, dataToSend.Length);
                 // Lägg till det enskilda meddelandet i databasen för varje mottagare
                AddSingleMessageToDB(kvp.Value, message);
            }

        }
    }

    static void SendPrivateMessage(string parameters, NetworkStream senderStream)
    {
        // Dela upp parametrarna för det privata meddelandet (antagande av att användarnamn och meddelande är skilda av ett mellanslag)
        string[] data = parameters.Split(" ");

         // Kontrollera att det finns minst två delar i parametrarna
        if (data.Length < 2)
        {
            Console.WriteLine("Incorrect private message format.");
            return;
        }
        // Hämta användarnamnet för avsändaren baserat på nätverksströmmen
        string sender = GetUsernameByStream(senderStream);
        // Hämta användarnamnet för mottagaren och meddelandet från parametrarna
        string recipient = data[0];
        string message = sender + ": " + parameters.Substring(recipient.Length).Trim();

        // Kontrollera om mottagaren är online
        if (userStreams.TryGetValue(recipient, out NetworkStream recipientStream))
        {
            // Konvertera meddelandet till byte-array för att skicka över nätverket
            byte[] dataToSend = Encoding.ASCII.GetBytes(message);
            recipientStream.Write(dataToSend, 0, dataToSend.Length);
        }
        else
        {
            // Skriv ut meddelande om att användaren inte hittades eller är offline
            Console.WriteLine($"User '{recipient}' not found or offline.");
        }
        // Lägg till det enskilda meddelandet i databasen
        AddSingleMessageToDB(recipientStream, message);
    }

    static IMongoCollection<User> FetchMongoUser()
    {
        // Sätt upp anslutningsinformationen för MongoDB-databasen
        const string newpass = "KokxLPCVbH0hKrp2";
        string connectionUri = "mongodb+srv://mattiashummer:" + newpass + "@cluster0.y5yh9uz.mongodb.net/?retryWrites=true&w=majority";

        // Skapa inställningar för MongoDB-klienten baserat på anslutningsinformationen
        var settings = MongoClientSettings.FromConnectionString(connectionUri);
        // Ange ServerApi-fältet i inställningsobjektet till stabil API-version 1
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        // Skapa en ny klient och anslut till servern
        var client = new MongoClient(settings);

        // Skicka en ping för att bekräfta en lyckad anslutning
        try
        {
            var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        // Anslut till den önskade databasen
        var database = client.GetDatabase("testing");
        // Anslut till den önskade kollektionen (i detta fall, "users")
        IMongoCollection<User> collection = database.GetCollection<User>("users");

        return collection;
    }


    static string GetUsernameByStream(NetworkStream stream)
    {
        // Loopa igenom varje par (Key-Value) i userStreams-dictionaryn
        foreach (var kvp in userStreams)
        {
            // Kontrollera om nätverksströmmen matchar den aktuella användaren i dictionaryn
            if (kvp.Value == stream)
            {
                // Returnera användarnamnet om nätverksströmmen matchar
                return kvp.Key;
            }
        }
        // Returnerar null om ingen matchning hittades
        return null; 
    }

    static string AddSingleMessageToDB(NetworkStream stream, string message)
    {
        // Hämta användarnamnet baserat på nätverksströmmen
        string username = GetUsernameByStream(stream);

        // Sätt upp anslutningsinformationen för MongoDB-databasen
        const string newpass = "KokxLPCVbH0hKrp2";
        string connectionUri = "mongodb+srv://mattiashummer:" + newpass + "@cluster0.y5yh9uz.mongodb.net/?retryWrites=true&w=majority";

        // Skapa inställningar för MongoDB-klienten baserat på anslutningsinformationen
        var settings = MongoClientSettings.FromConnectionString(connectionUri);

         // Ange ServerApi-fältet i inställningsobjektet till stabil API-version 1
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        // Skapa en ny klient och anslut till servern
        var client = new MongoClient(settings);

        // Skicka en ping för att bekräfta en lyckad anslutning
        try
        {
            var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        // Anslut till den önskade databasen
        var database = client.GetDatabase("testing");

        // Anslut till den önskade kollektionen (i detta fall, "messages")
        IMongoCollection<Messages> messageCollection = database.GetCollection<Messages>("messages");

        // Skapa filter för att hitta rätt användare
        var filter = Builders<Messages>.Filter.Eq(message => message.UserName, username);

        // Skapa uppdatering för att lägga till det nya meddelandet
        var update = Builders<Messages>.Update.Push(message => message.UserMessages, message);

        // Hantera antalet meddelanden för varje användare
        int maxMessages = 29;
        var userMessages = FetchMongoMessages(username).UserMessages;

        // Om antalet meddelanden överstiger maxMessages, ta bort det äldsta meddelandet
        if (userMessages.Count > maxMessages)
        {
            var oldestMessage = userMessages.FirstOrDefault();

            // Uppdatera collectionen för att ta bort det äldsta meddelandet
            messageCollection.UpdateOne(
                Builders<Messages>.Filter.Eq("UserName", username),
                Builders<Messages>.Update.Pull("UserMessages", oldestMessage)
            );
        }
        // Uppdatera collectionen med det nya meddelandet
        messageCollection.UpdateOne(filter, update);

        // Returnera null (om ingen särskild returinformation behövs)
        return null;
    }




    static Messages FetchMongoMessages(string username)
    {
        // Sätt upp anslutningsinformationen för MongoDB-databasen
        const string newpass = "KokxLPCVbH0hKrp2";
        string connectionUri = "mongodb+srv://mattiashummer:" + newpass + "@cluster0.y5yh9uz.mongodb.net/?retryWrites=true&w=majority";
        
        // Skapa inställningar för MongoDB-klienten baserat på anslutningsinformationen
        var settings = MongoClientSettings.FromConnectionString(connectionUri);
        
        // Ange ServerApi-fältet i inställningsobjektet till stabil API-version 1
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        
        // Skapa en ny klient och anslut till servern
        var client = new MongoClient(settings);
        
        // Skicka en ping för att bekräfta en lyckad anslutning
        try
        {
            var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        // anslut till den önskade databasen
        var database = client.GetDatabase("testing");
        
        // Anslut till den önskade kollektionen (i detta fall, "messages")
        IMongoCollection<Messages> collection = database.GetCollection<Messages>("messages");

        // Hämta befintliga meddelanden för användaren från databasen
        Messages existingMessages = collection.Find(x => x.UserName == username).FirstOrDefault();
        
        // Om det inte finns befintliga meddelanden för användaren, skapa en temporär instans och lägg till den i databasen
        if (existingMessages == null)
        {
            Random random = new Random();
            int randomTal = random.Next(1, 1000);
            Messages tempMessage = new Messages(randomTal, username, new List<string>());
            collection.InsertOne(tempMessage);
            return tempMessage;
        }
        // Returnera befintliga meddelanden om de finns
        return existingMessages;
    }


    // Dictionary för att koppla kommandon till åtgärder (Actions)
    static Dictionary<string, Action<string, NetworkStream>> commandActions = new Dictionary<string, Action<string, NetworkStream>>()
                {
                    { "register", RegisterUser}, //Koppla "register"-kommandot till RegisterUser-metoden
                    { "login", LoginUser}, //Koppla "login"-kommandot till LoginUser-metoden
                    { "send", SendMessage}, //Koppla "send"-kommandot till SendMessage-metoden
                    { "sendPrivate", SendPrivateMessage}, // Koppl "sendPrivate"-klommandot till SendPrivateMessage-metoden
                };

    // Dictionary för att hålla koll på vilken nätverksström som är kopplad till varje användare
    static Dictionary<string, NetworkStream> userStreams = new Dictionary<string, NetworkStream>();

}
// Klass som representerar en användare med tre egenskaper: Id, UserName och Password
class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
// Klass som representerar meddelanden för en användare med tre egenskaper: Id, UserName och en lista av användarmeddelanden
class Messages
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public List<string> UserMessages { get; set; }

    // Konstruktor för Messages-klassen som används för att skapa en instans av klassen med specifika värden
    public Messages(int id, string userName, List<string> userMessages)
    {
        Id = id;
        UserName = userName;
        UserMessages = userMessages;
    }
}


