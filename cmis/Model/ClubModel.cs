using System.Collections.Generic;

namespace cmis.Model
{

    public class MessageModel
    {
        public string user_id { get; set; }
        public int club_id { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
    }
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
        public string Approval { get; set; }
    }

    public class CreateEventModel
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
        public string event_description { get; set; }
        public int club_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string pic_url { get; set; }
        public string approval { get; set; } = "pending";
    }

    public class CreateClubModel
    {
        public int club_id { get; set; }
        public string club_name { get; set; }
        public string club_description { get; set; }
        public string club_logo_url { get; set; }
    }


    public class EventRegistrationModel
    {
        public int register_id { get; set; }
        public int event_id { get; set; }
        public string user_id { get; set; }
        public DateTime registration_date { get; set; }
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
    public class ElectedPresidentModel
    {
        public int CandidateId { get; set; }                 
        public string CandidateName { get; set; }             
        public string CandidateUserId { get; set; }           
        public string CandidateProfilePic { get; set; }       
        public string ClubName { get; set; }                   
        public int ClubId { get; set; }                      
        public int TotalVotes { get; set; }                   
        public decimal VotePercentage { get; set; }           
        public int ElectionId { get; set; }                   
    }

    public class Candidate
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateUserId { get; set; }
        public string CandidateProfilePic { get; set; }
        public string ClubName { get; set; }
        public int ClubId { get; set; }
        public int ElectionId { get; set; }
    }

    public class ElectionResultModel
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateUserId { get; set; }
        public string CandidateProfilePic { get; set; }
        public string ClubName { get; set; }
        public int ClubId { get; set; }
        public int TotalVotes { get; set; }
        public double VotePercentage { get; set; }
        public int ElectionId { get; set; }
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
    public class VoteSaveModel
    {
        public int election_id { get; set; }
        public int candidate_id { get; set; }
        public DateTime? vote_date { get; set; }
        public string voter_id { get; set; }
    }

    public class Election
    {
        public int election_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int? club_id { get; set; }
        public string status { get; set; }
    }


    public class AnnouncementSaveModel
    {
        public string announcement_by_id { get; set; }
        public int? club_id { get; set; } 
        public string? user_id { get; set; }
        public DateTime created_at { get; set; }
        public string announcement_text { get; set; }
        public string announcement_title { get; set; }
    }

}
