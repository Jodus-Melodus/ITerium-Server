using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main(string[] args)
    // Starts up the server and listen for incoming clients
    {
        TcpListener server = new(IPAddress.Any, 12345);
        server.Start();

        Log("Server is listening...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Log($"Client connected: {((IPEndPoint)client.Client.RemoteEndPoint).Port}");

            Thread clientThread = new(HandleClient);
            clientThread.Start(client);
        }
    }

    static void HandleClient(object clientObj)
    // Accepts the clients and handle their incoming requests
    {
        TcpClient tcpClient = (TcpClient)clientObj;
        NetworkStream clientStream = tcpClient.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = clientStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Log($"Received: {receivedMessage}");

            string[] request = receivedMessage.Split(" ");

            switch (request[0])
            {
                case "marketplace":
                    SendMarketplace(clientStream);
                    break;
                case "balance":
                    if (request.Length > 1)
                    {
                        SendBalance(clientStream, request[1]);
                    }
                    break;
                case "profile_request":
                    if (request.Length > 2)
                    {
                        HandleProfileRequest(clientStream, request[1], request[2]);
                    }
                    break;
                default:
                    Log($"Invalid request : \"{request}\"");
                    break;
            }
        }
        tcpClient.Close();
    }

    private static void HandleProfileRequest(NetworkStream clientStream, string studentCode, string password)
    // If the studentCode does not exist it creates the profile otherwise it cancels and notifies the client that it's request has been canceled
    {
        string path = "users\\" + studentCode + ".csv";
        bool approved = !Path.Exists(path);
        byte[] responseBuffer = Encoding.ASCII.GetBytes(approved.ToString());

        if (approved)
        {
            Log($"Approved profile request from : {studentCode}");
            WriteFile(path, "0");

            AppendFile("users\\users.csv", $"{studentCode}, {password}");
        }
        else
        {
            Log($"Rejected profile request from : {studentCode}");
        }

        clientStream.Write(responseBuffer, 0, responseBuffer.Length);
    }

    static void SendMarketplace(NetworkStream clientStream)
    // Sends the marketplace file to the client
    {
        string[] marketplacePrices = ReadFile("marketplace.csv");
        byte[] responseBuffer = Encoding.ASCII.GetBytes(string.Join("\n", marketplacePrices));

        clientStream.Write(responseBuffer, 0, responseBuffer.Length);
    }

    static void SendBalance(NetworkStream clientStream, string studentCode)
    // sends the balance of the client
    {
        string path = "users\\" + studentCode + ".csv";

        if (Path.Exists(path))
        {
            string[] clientBalance = ReadFile(path);
            byte[] responseBuffer = Encoding.ASCII.GetBytes(string.Join("\n", clientBalance));

            clientStream.Write(responseBuffer, 0, responseBuffer.Length);
        }
        else
        {
            byte[] responseBuffer = Encoding.ASCII.GetBytes("User not registered");

            clientStream.Write(responseBuffer, 0, responseBuffer.Length);
            Log("Client is not registered");
        }
    }

    static string[] ReadFile(string path)
    {
        try
        {
            return File.ReadAllLines(path);
        }
        catch (Exception e)
        {
            Log($"An error occurred: {e.Message}");
            return [];
        }
    }

    static void AppendFile(string filePath, string content)
    {
        try
        {
            File.AppendAllText(filePath, content + "\n");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }
    }

    static void WriteFile(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content);
        }
        catch (Exception e)
        {
            Log($"An error occurred: {e.Message}");
        }
    }

    static void Log(string logMessage)
    {
        string msg = $"[{DateTime.Now}] " + logMessage;
        AppendFile("server.log", msg);
        Console.WriteLine(msg);
    }
}