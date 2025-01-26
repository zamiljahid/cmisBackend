using System;
namespace cmis.Model
{
    public class Message
    {
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public int? ClubId { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }
    }

}

