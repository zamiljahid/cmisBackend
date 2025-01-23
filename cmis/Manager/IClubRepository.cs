﻿using cmis.Model;
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

    }
}
