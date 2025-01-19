using Google.Protobuf.WellKnownTypes;

namespace cmis.Model
{
    public class MessageModel
    {
        public string user_id { get; set; }
        public int club_id { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
    }
}
