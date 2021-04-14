using System;

namespace WSChat
{
    class Program
    {
        private const string ConnectionString = "ws://127.0.0.1:";

        static void Main(string[] args)
        {
            Console.WriteLine("WSChat\n");
            if (args.Length == 0)
            {
                Console.WriteLine("Missing Port argumnet");
                Console.WriteLine("Usage: WSChat <port>\n");
                return;
            }
            string port = args[0];


            Console.WriteLine("What's your nickname: ");
            string nick = Console.ReadLine();

            ChatService chatService = new(ConnectionString, port, nick, "cheerio");
            Console.WriteLine("Type: cheerio  to finish the chat session and exit.\n");

            // Lets get chatting!
            chatService.StartService();
        }
    }
}
