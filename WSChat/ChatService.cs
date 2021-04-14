using System;
using System.IO;
using System.Threading;
using Fleck;
using MessagePack;

namespace WSChat
{
    public enum CurrentMode
    {
        Client,
        Server
    };

    public struct AdminMessage
    {
        public const string Joined = "Joined";
        public const string Left = "BYE!";
    }

    public class ChatService
    {
        public static int ReceiveBufferSize { get; set; } = 8192;

        public string ConnectionString { get; set; }
        public string ConnectionPort { get; set; }
        public string UserNick { get; set; }
        public string QuitKeyword { get; set; }
        public CurrentMode CurrentMode { get; set; }

        public bool ShouldExit { get; set; }

        public ServerChat ServerChat { get; set; }

        public ChatService(string connectionString, string port, string nick, string quitKeyword)
        {
            // Initialization
            FleckLog.Level = LogLevel.Error;
            ConnectionString = connectionString + port;
            UserNick = nick;
            QuitKeyword = quitKeyword;
        }

        public void StartService()
        {
            // Try to connect as the client, if we can't find a server, catch
            // the exception, and then continue as the server.

            // CLIENT
            ClientChat clientChat = new(this, ConnectionString);
            if (ConnectClientChat(ref clientChat))
            {
                CurrentMode = CurrentMode.Client;
                Console.WriteLine($"Hi, {UserNick}!, we've connected you to the chat server.\n");
                clientChat.SendMessage("Joined.");

                var chatTS = new ThreadStart(clientChat.ChatLoopAsync);
                var chatThread = new Thread(chatTS);
                chatThread.Start();

                string message;
                while (true)
                {
                    Console.Write("> ");
                    message = Console.ReadLine();
                    if (message == QuitKeyword || ShouldExit)
                    {
                        // Send logout message
                        clientChat.SendMessage("BYE!");
                        break;
                    }

                    // Send the message to the server
                    clientChat.SendMessage(message);
                }

                clientChat.DisconnectAsync().Wait();
                Console.WriteLine($"Bye {UserNick}!\n");
                return;
            }
            else
            // SERVER
            {
                try
                {
                    CurrentMode = CurrentMode.Server;
                    Console.WriteLine($"Hi, {UserNick}!, looks like you'll be the message server today\n");
                    ServerChat = new(this);
                    ServerChat.Connect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error happened while attempting to connect the server.");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void TerminateService()
        {
            Console.WriteLine("Emergency exit! Halt the machines!");
            ShouldExit = true;
        }

        static bool ConnectClientChat(ref ClientChat clientChat)
        {
            try
            {
                clientChat.ConnectAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DisplayMessage(Stream inputStream)
        {
            try
            {
                var chatMessage = MessagePackSerializer.Deserialize<ChatMessage>(inputStream);

                // Don't display the message if the sender is this user.
                if (chatMessage.Author == UserNick)
                {
                    return;
                }

                Console.WriteLine($"[{chatMessage.Author} at {chatMessage.TimeStamp.ToShortTimeString()}] said: {chatMessage.Message}");
                Console.Write("> ");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deserializing message");
                Console.WriteLine(ex.Message);
            }
        }

        public void DisplayMessage(byte[] inputData)
        {
            DisplayMessage(new MemoryStream(inputData));
        }

        public static ChatMessage DecodeMessage(byte[] inputData)
        {
            try
            {
                return MessagePackSerializer.Deserialize<ChatMessage>(new MemoryStream(inputData));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deserializing message");
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public byte[] EncodeMessage(string message)
        {
            ChatMessage chatMessage = new(UserNick, message, DateTime.Now);
            return MessagePackSerializer.Serialize(chatMessage);
        }
    }
}
