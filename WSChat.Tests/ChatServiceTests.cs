using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WSChat.Tests
{
    [TestClass]
    public class ChatServiceTests
    {
        private const string ConnectionString = "ws://127.0.0.1:";
        private static ChatService _chatService;
        private static ServerChat _serverChat;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // Initalization code goes here
            string port = "8181";
            _chatService = new(ConnectionString, port, "helia", "cheerio");
            _serverChat = new(_chatService);
            _serverChat.Connect(waitForInput: false);
        }

        [TestMethod]
        public void ServerShouldRunWhenNotRunning()
        {
            Assert.IsTrue(_serverChat.IsConnected);
        }

        [TestMethod]
        public void ClientShouldRunIfServerPresent()
        {
            ClientChat clientChat = new(_chatService, ConnectionString);
            bool connectionStatus = clientChat.ConnectAsync();

            Assert.IsTrue(connectionStatus);
            clientChat.DisconnectAsync().Wait();
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            _serverChat.Disconnect();
        }
    }
}
