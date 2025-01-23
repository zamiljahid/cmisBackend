using System.Collections.Generic;

namespace cmis.Model
{
    public class ClubModel
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public string ClubDescription { get; set; }
        public string ClubLogoUrl { get; set; }
        public List<EventModel> Events { get; set; } 
    }

    public class EventModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string PicUrl { get; set; }
        public string Status { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; } 
    }

    public class MembershipModel
    {
        public string Id { get; set; }
        public string MemberName { get; set; }
        public string ProfilePic { get; set; }
        public string Position { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public int ElectionParticipation { get; set; }  
        public int EventParticipation { get; set; }    
        public int MessageParticipation { get; set; }
        public string ClubNames { get; set; }

    }

    public class TopClubModel
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public int TotalElectionParticipation { get; set; }
        public int TotalEventParticipation { get; set; }
        public int TotalMessageParticipation { get; set; }
        public int TotalMembers { get; set; }
        public int TotalPresidents { get; set; }
    }

}
