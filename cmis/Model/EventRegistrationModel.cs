using System;

namespace cmis.Model
{
    public class EventRegistrationModel
    {
        public int register_id { get; set; }
        public int event_id { get; set; }
        public string user_id { get; set; }
        public DateTime registration_date { get; set; }
    }
}
