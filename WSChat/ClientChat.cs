using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WSChat
{
    public class ClientChat : IDisposable
    {
        public ChatService ChatService { get; set; }
        private ClientWebSocket _connection;
        private CancellationTokenSource CTS;

        private readonly string _connectionString;

        public ClientChat(ChatService service, string connectionString)
        {
            ChatService = service;
            _connectionString = connectionString;
        }

        public bool ConnectAsync()
        {
            if (_connection is not null)
            {
                if (_connection.State == WebSocketState.Open)
                {
                    return true;
                }
                else
                {
                    _connection.Dispose();
                }
            }
            _connection = new ClientWebSocket();

            if (CTS is not null)
            {
                CTS.Dispose();
            }
            CTS = new CancellationTokenSource();

            _connection.ConnectAsync(new Uri(_connectionString), CTS.Token).Wait();
            if (_connection.State == WebSocketState.Open)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Should close all the parts of the socket connection to be able
        // to close the socket cleanly.
        public async Task DisconnectAsync()
        {
            if (_connection is null)
            {
                return;
            }
            if (_connection.State == WebSocketState.Open)
            {
                CTS.CancelAfter(TimeSpan.FromSeconds(2));
                await _connection.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await _connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }

            _connection.Dispose();
            CTS.Dispose();
        }

        public async void ChatLoopAsync()
        {
            var loopToken = CTS.Token;
            MemoryStream outputStream = null;
            var buffer = new byte[ChatService.ReceiveBufferSize];

            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(ChatService.ReceiveBufferSize);
                    WebSocketReceiveResult receiveResult;
                    do
                    {
                        receiveResult = await _connection.ReceiveAsync(buffer, CTS.Token);
                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                        {
                            outputStream.Write(buffer, 0, receiveResult.Count);
                        }
                    }
                    while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    outputStream.Position = 0;
                    ReceiveMessage(outputStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error interruped the connection.");
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
            finally
            {
                outputStream?.Dispose();
            }
        }

        private void ReceiveMessage(Stream inputStream)
        {
            ChatService.DisplayMessage(inputStream);
        }

        public void SendMessage(string message)
        {
            var token = CTS.Token;
            _connection.SendAsync(ChatService.EncodeMessage(message), WebSocketMessageType.Binary, true, token).Wait();
        }

        public void Dispose() => DisconnectAsync().Wait();
    }
}
