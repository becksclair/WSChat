using System;
using System.Collections.Generic;
using System.Linq;
using Fleck;

namespace WSChat
{
    public class ServerChat
    {
        public ChatService ChatService { get; set; }
        public bool IsConnected { get; set; }
        public List<IWebSocketConnection> AllSockets { get; set; }

        private WebSocketServer _server;

        public ServerChat(ChatService service)
        {
            // Receive our chat service to work on
            ChatService = service;
            AllSockets = new List<IWebSocketConnection>();
        }

        public void Connect(bool waitForInput = true)
        {
            _server = new WebSocketServer(ChatService.ConnectionString)
            {
                RestartAfterListenError = true
            };
            Console.Write("> ");
            _server.Start(socket =>
            {
                socket.OnOpen   = () => AllSockets.Add(socket);
                socket.OnClose  = () => AllSockets.Remove(socket);
                socket.OnBinary = messageData =>
                {
                    var chatMessage = ChatService.DecodeMessage(messageData);
                    if (chatMessage is not null)
                    {
                        if (chatMessage.Message.Contains(AdminMessage.Joined))
                        {
                            Console.WriteLine($"User: {chatMessage.Author} has joined");
                            Console.Write("> ");
                        }
                        else if (chatMessage.Message.Contains(AdminMessage.Left))
                        {
                            Console.WriteLine($"User: {chatMessage.Author} has left");
                            Console.Write("> ");
                        }
                        else
                        {
                            ChatService.DisplayMessage(messageData);
                        }
                    }

                    // Broadcast the message to all listeing clients
                    AllSockets.ToList().ForEach(s => s.Send(messageData));
                };
            });
            IsConnected = true;

            // Send Admin messages to all participants
            if (waitForInput)
            {
                var message = Console.ReadLine();
                while (message != ChatService.QuitKeyword && !ChatService.ShouldExit)
                {
                    foreach (var socket in AllSockets.ToList())
                    {
                        socket.Send(ChatService.EncodeMessage($"ADMIN ] {message}"));
                    }
                    Console.Write("> ");
                    message = Console.ReadLine();
                }
            }
        }

        public void Disconnect()
        {
            _server.Dispose();
            IsConnected = false;
        }
    }
}
