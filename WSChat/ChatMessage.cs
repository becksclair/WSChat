using System;
using MessagePack;

namespace WSChat
{
    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)]
        public string Author { get; set; }
        [Key(1)]
        public string Message { get; set; }
        [Key(2)]
        public DateTime TimeStamp { get; set; }

        public ChatMessage(string author, string message, DateTime timeStamp)
        {
            Author = author;
            Message = message;
            TimeStamp = timeStamp;
        }
    }
}
