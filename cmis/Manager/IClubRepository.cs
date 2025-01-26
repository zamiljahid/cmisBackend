using cmis.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cmis.Manager
{
    public interface IClubRepository
    {
        Task<List<ClubModel>> GetAllClubsAsync();
        Task<List<EventModel>> GetUpcomingAndOngoingEventsAsync();
        Task<List<EventModel>> GetPendingEventsAsync();
        Task<List<MembershipModel>> GetMembersByClubIdAsync(int clubId);
        Task<List<MembershipModel>> GetTopPresidentsAsync();
        Task<List<TopClubModel>> GetTopClubsAsync();
        string SaveEventRegistration(EventRegistrationModel eventRegistrationModel);
        string CreateEvent(CreateEventModel createEventModel);
        string SaveAnnouncement(AnnouncementSaveModel announcementModel);
        string SaveVote(VoteSaveModel voteModel);
        string CreateClub(CreateClubModel createClubModel);
        string UpdateEventStatus(int eventId, string action);
        string UpdateUserRole(string userId, string action);
        string RegisterClubMember(string userId, int clubId);
        Task<List<ElectedPresidentModel>> GetElectedPresidentsAsync();
        Task<List<ElectionResultModel>> GetElectionResultsAsync(int clubId);
        Task<List<Candidate>> GetCandidatesForVotingAsync(int clubId);
        bool CheckIfVoterExists(string voterId, int electionId);
        bool CheckIfUserRegisteredForEvent(int eventId, string userId);
        Task<List<Message>> GetMessagesByClubIdAsync(int clubId);
        Task<int> AddMessageAsync(Message message);
        string GetLatestElectionStatus();
    }
}
