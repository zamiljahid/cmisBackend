using cmis.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace cmis.Manager
{
    public class ClubRepository : IClubRepository
    {
        private readonly string _connectionString;

        public ClubRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySQLConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<List<ClubModel>> GetAllClubsAsync()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sqlClubs = @"
            SELECT 
                club_id AS ClubId, 
                club_name AS ClubName, 
                club_description AS ClubDescription, 
                club_logo_url AS ClubLogoUrl 
            FROM 
                club";

                string sqlEvents = @"
    SELECT 
        e.event_id AS EventId, 
        e.event_name AS EventName, 
        e.event_description AS EventDescription, 
        e.start_date AS StartDate, 
        e.end_date AS EndDate, 
        e.club_id AS ClubId,
        e.pic_url AS PicUrl, 
        e.approval AS Approval,
        CASE 
            WHEN NOW() BETWEEN e.start_date AND e.end_date THEN 'On Going Event'
            WHEN NOW() < e.start_date THEN 'Upcoming Event'
            ELSE 'Previous Event'
        END AS Status
    FROM 
        events e
    WHERE 
        e.approval = 'approved'";

                var clubs = (await connection.QueryAsync<ClubModel>(sqlClubs)).ToList();

                if (clubs.Any())
                {
                    var events = (await connection.QueryAsync<EventModel>(sqlEvents)).ToList();

                    foreach (var club in clubs)
                    {
                        club.Events = events.Where(e => e.ClubId == club.ClubId).ToList();
                    }
                }

                return clubs;
            }
        }

        public async Task<List<EventModel>> GetUpcomingAndOngoingEventsAsync()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
            SELECT 
                e.event_id AS EventId, 
                e.event_name AS EventName, 
                e.event_description AS EventDescription, 
                e.start_date AS StartDate, 
                e.end_date AS EndDate, 
                e.club_id AS ClubId,
                e.pic_url AS PicUrl, 
                c.club_name AS ClubName,
                e.approval AS Approval,
                CASE 
                    WHEN NOW() BETWEEN e.start_date AND e.end_date THEN 'On Going Event'
                    WHEN NOW() < e.start_date THEN 'Upcoming Event'
                END AS Status
            FROM 
                events e
            INNER JOIN 
                club c ON e.club_id = c.club_id
            WHERE 
                NOW() <= e.end_date
                AND e.approval = 'approved'"; 
                var events = (await connection.QueryAsync<EventModel>(sql)).ToList();

                return events;
            }
        }

        public async Task<List<EventModel>> GetPendingEventsAsync()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
            SELECT 
                e.event_id AS EventId, 
                e.event_name AS EventName, 
                e.event_description AS EventDescription, 
                e.start_date AS StartDate, 
                e.end_date AS EndDate, 
                e.club_id AS ClubId,
                e.pic_url AS PicUrl, 
                c.club_name AS ClubName,
                e.approval AS Approval,
                CASE 
                    WHEN NOW() BETWEEN e.start_date AND e.end_date THEN 'On Going Event'
                    WHEN NOW() < e.start_date THEN 'Upcoming Event'
                END AS Status
            FROM 
                events e
            INNER JOIN 
                club c ON e.club_id = c.club_id
            WHERE 
                e.approval = 'pending'";

                var events = (await connection.QueryAsync<EventModel>(sql)).ToList();

                return events;
            }
        }

        public async Task<List<ElectionResultModel>> GetElectionResultsAsync(int clubId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
            SELECT 
                c.candidate_id AS CandidateId,
                u.name AS CandidateName,
                u.user_id AS CandidateUserId,
                u.profile_pic_url AS CandidateProfilePic,
                cl.club_name AS ClubName,
                cl.club_id AS ClubId,
                COUNT(v.vote_id) AS TotalVotes,
                ROUND((COUNT(v.vote_id) / MAX(tm.total_members)) * 100, 2) AS VotePercentage,
                e.election_id AS ElectionId
            FROM 
                election e
            LEFT JOIN 
                candidate c ON e.election_id = c.election_id
            LEFT JOIN 
                vote v ON c.candidate_id = v.candidate_id
            LEFT JOIN 
                user u ON c.user_id = u.user_id
            LEFT JOIN 
                club_members cm ON u.membership_id = cm.membership_id
            LEFT JOIN 
                club cl ON cm.club_id = cl.club_id
            LEFT JOIN 
                (
                    SELECT 
                        c2.club_id, 
                        COUNT(DISTINCT cm2.membership_id) AS total_members  
                    FROM 
                        club c2
                    LEFT JOIN 
                        club_members cm2 ON c2.club_id = cm2.club_id 
                    GROUP BY 
                        c2.club_id
                ) tm ON cl.club_id = tm.club_id
            WHERE 
                e.status = 'elected'  
                AND e.end_date < NOW()
                AND cl.club_id = @ClubId 
            GROUP BY 
                c.candidate_id, u.name, u.user_id, u.profile_pic_url, cl.club_name, cl.club_id, e.election_id
            ORDER BY 
                TotalVotes DESC";

                var results = (await connection.QueryAsync<ElectionResultModel>(sql, new { ClubId = clubId })).ToList();

                return results;
            }
        }

        public async Task<List<MembershipModel>> GetMembersByClubIdAsync(int clubId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sqlMembershipIds = @"
            SELECT membership_id 
            FROM club_members
            WHERE club_id = @ClubId";

                var membershipIds = (await connection.QueryAsync<int>(sqlMembershipIds, new { ClubId = clubId })).ToList();

                if (!membershipIds.Any())
                    return new List<MembershipModel>();

                string sqlMembers = @"
            SELECT 
                u.user_id AS Id,
                u.name AS MemberName,
                r.role_name AS Position,  
                u.contact AS Contact,
                u.email AS Email,
                IFNULL(COUNT(DISTINCT c.candidate_id), 0) AS ElectionParticipation,
                IFNULL(COUNT(DISTINCT er.event_id), 0) AS EventParticipation,
                IFNULL(COUNT(DISTINCT m.message_id), 0) AS MessageParticipation
            FROM 
                user u
            JOIN 
                role r ON u.role_id = r.role_id  
            JOIN 
                club_members cm ON u.user_id = cm.user_id
            LEFT JOIN 
                candidate c ON u.user_id = c.user_id 
            LEFT JOIN 
                event_registration er ON u.user_id = er.user_id 
            LEFT JOIN 
                messages m ON u.user_id = m.user_id 
            WHERE 
                cm.club_id = @ClubId
            AND 
                u.membership_id IN @MembershipIds
            GROUP BY 
                u.user_id
            ORDER BY 
                ElectionParticipation DESC, 
                EventParticipation DESC, 
                MessageParticipation DESC";

                var members = (await connection.QueryAsync<MembershipModel>(sqlMembers, new { ClubId = clubId, MembershipIds = membershipIds })).ToList();

                return members;
            }
        }

        public async Task<List<MembershipModel>> GetTopPresidentsAsync()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
        SELECT 
            u.user_id AS Id,
            u.name AS MemberName,
            u.profile_pic_url AS ProfilePic,
            r.role_name AS Position,  
            u.contact AS Contact,
            u.email AS Email,
            IFNULL(COUNT(DISTINCT c.candidate_id), 0) AS ElectionParticipation,
            IFNULL(COUNT(DISTINCT er.event_id), 0) AS EventParticipation,
            IFNULL(COUNT(DISTINCT m.message_id), 0) AS MessageParticipation,
            GROUP_CONCAT(DISTINCT cl.club_name) AS ClubNames
        FROM 
            user u
        JOIN 
            role r ON u.role_id = r.role_id
        JOIN 
            club_members cm ON u.user_id = cm.user_id
        LEFT JOIN 
            candidate c ON u.user_id = c.user_id 
        LEFT JOIN 
            event_registration er ON u.user_id = er.user_id 
        LEFT JOIN 
            messages m ON u.user_id = m.user_id 
        JOIN 
            club cl ON cm.club_id = cl.club_id  -- Join with club table to get the club name
        WHERE 
            r.role_id = 2  -- Filter for President role
        GROUP BY 
            u.user_id
        ORDER BY 
            ElectionParticipation DESC, 
            EventParticipation DESC, 
            MessageParticipation DESC";

                var presidents = (await connection.QueryAsync<MembershipModel>(sql)).ToList();

                return presidents;
            }
        }

        public async Task<List<TopClubModel>> GetTopClubsAsync()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
                SELECT 
                    cm.club_id AS ClubId,
                    c.club_name AS ClubName,
                    COUNT(DISTINCT c1.candidate_id) AS TotalElectionParticipation,
                    COUNT(DISTINCT er1.event_id) AS TotalEventParticipation,
                    COUNT(DISTINCT m1.message_id) AS TotalMessageParticipation,
                    COUNT(DISTINCT cm.user_id) AS TotalMembers,
                    COUNT(DISTINCT CASE WHEN r.role_id = 2 THEN u.user_id END) AS TotalPresidents
                FROM 
                    club_members cm
                JOIN 
                    club c ON cm.club_id = c.club_id
                JOIN 
                    user u ON cm.user_id = u.user_id
                JOIN 
                    role r ON u.role_id = r.role_id
                LEFT JOIN 
                    candidate c1 ON u.user_id = c1.user_id 
                LEFT JOIN 
                    event_registration er1 ON u.user_id = er1.user_id 
                LEFT JOIN 
                    messages m1 ON u.user_id = m1.user_id 
                GROUP BY 
                    cm.club_id, c.club_name
                ORDER BY 
                    TotalElectionParticipation DESC, 
                    TotalEventParticipation DESC, 
                    TotalMessageParticipation DESC";

                var topClubs = (await connection.QueryAsync<TopClubModel>(sql)).ToList();

                return topClubs;
            }
        }

        public string SaveEventRegistration(EventRegistrationModel eventRegistrationModel)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string sql = "INSERT INTO event_registration (event_id, user_id, registration_date) VALUES (@event_id, @user_id,@registration_date);";
                    int rowsAffected = dbConnection.Execute(sql, new { event_id = eventRegistrationModel.event_id, user_id = eventRegistrationModel.user_id, registration_date= eventRegistrationModel.registration_date });
                    return "Success"; 
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string CreateEvent(CreateEventModel createEventModel)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string sql = "INSERT INTO events (event_name, event_description, club_id, start_date, end_date, pic_url, approval) " +
                                 "VALUES (@event_name, @event_description, @club_id, @start_date, @end_date, @pic_url, 'pending');";

                    int rowsAffected = dbConnection.Execute(sql, new
                    {
                        event_name = createEventModel.event_name,
                        event_description = createEventModel.event_description,
                        club_id = createEventModel.club_id,
                        start_date = createEventModel.start_date,
                        end_date = createEventModel.end_date,
                        pic_url = createEventModel.pic_url
                    });

                    return "Success!";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string CreateClub(CreateClubModel createClubModel)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string sql = "INSERT INTO club (club_name, club_description, club_logo_url) " +
                                 "VALUES (@club_name, @club_description, @club_logo_url);";

                    int rowsAffected = dbConnection.Execute(sql, new
                    {
                        club_name = createClubModel.club_name,
                        club_description = createClubModel.club_description,
                        club_logo_url = createClubModel.club_logo_url
                    });

                    return "Club created successfully!";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Candidate>> GetCandidatesForVotingAsync(int clubId)
    {
        using (var connection = CreateConnection())
        {
            connection.Open();

            string query = @"
                SELECT
                    c.candidate_id AS CandidateId,
                    u.name AS CandidateName,
                    u.user_id AS CandidateUserId,
                    u.profile_pic_url AS CandidateProfilePic,
                    cl.club_name AS ClubName,
                    cl.club_id AS ClubId,
                    e.election_id AS ElectionId
                FROM 
                    election e
                LEFT JOIN 
                    candidate c ON e.election_id = c.election_id
                LEFT JOIN 
                    user u ON c.user_id = u.user_id
                LEFT JOIN 
                    club_members cm ON u.membership_id = cm.membership_id
                LEFT JOIN 
                    club cl ON cm.club_id = cl.club_id
                WHERE 
                    e.status = 'voting'
                    AND e.end_date > NOW()
                    AND cl.club_id = @ClubId
                GROUP BY 
                    c.candidate_id, u.name, u.user_id, u.profile_pic_url, cl.club_name, cl.club_id, e.election_id
                ORDER BY 
                    CandidateName ASC";

            var candidates = (await connection.QueryAsync<Candidate>(query, new { ClubId = clubId })).ToList();

            return candidates;
        }
    }

        public bool CheckIfUserRegisteredForEvent(int eventId, string userId)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string query = "SELECT COUNT(*) AS count FROM event_registration WHERE event_id = @EventId AND user_id = @UserId";

                    int count = dbConnection.ExecuteScalar<int>(query, new { EventId = eventId, UserId = userId });

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking user registration for event.", ex);
            }
        }

        public bool CheckIfVoterExists(string voterId, int electionId)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string query = "SELECT COUNT(*) AS count FROM vote WHERE voter_id = @VoterId AND election_id = @ElectionId";

                    int count = dbConnection.ExecuteScalar<int>(query, new { VoterId = voterId, ElectionId = electionId });

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking voter existence.", ex);
            }
        }

        public async Task<List<ElectedPresidentModel>> GetElectedPresidentsAsync()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
        SELECT 
            c.candidate_id AS CandidateId,
            u.name AS CandidateName,
            u.user_id AS CandidateUserId,
            u.profile_pic_url AS CandidateProfilePic,
            cl.club_name AS ClubName,
            cl.club_id AS ClubId,
            COUNT(v.vote_id) AS TotalVotes,
            ROUND((COUNT(v.vote_id) / MAX(tm.total_members)) * 100, 2) AS VotePercentage,
            e.election_id AS ElectionId 
        FROM 
            election e
        LEFT JOIN 
            candidate c ON e.election_id = c.election_id
        LEFT JOIN 
            vote v ON c.candidate_id = v.candidate_id
        LEFT JOIN 
            user u ON c.user_id = u.user_id
        LEFT JOIN 
            club_members cm ON u.membership_id = cm.membership_id
        LEFT JOIN 
            club cl ON cm.club_id = cl.club_id
        LEFT JOIN 
            (
                SELECT 
                    c2.club_id, 
                    COUNT(DISTINCT cm2.membership_id) AS total_members  
                FROM 
                    club c2
                LEFT JOIN 
                    club_members cm2 ON c2.club_id = cm2.club_id 
                GROUP BY 
                    c2.club_id
            ) tm ON cl.club_id = tm.club_id 
        WHERE 
            e.status = 'voting' 
            AND e.end_date < NOW()
        GROUP BY 
            c.candidate_id, u.name, u.user_id, u.profile_pic_url, cl.club_name, cl.club_id, e.election_id
        HAVING 
            TotalVotes = (
                SELECT 
                    MAX(vote_count)
                FROM (
                    SELECT 
                        COUNT(v.vote_id) AS vote_count, 
                        c.candidate_id
                    FROM 
                        vote v
                    LEFT JOIN 
                        candidate c ON v.candidate_id = c.candidate_id
                    LEFT JOIN
                        election e ON c.election_id = e.election_id  
                    GROUP BY 
                        c.candidate_id
                ) AS vote_counts
            );
        ";

                var presidents = (await connection.QueryAsync<ElectedPresidentModel>(sql)).ToList();

                return presidents;
            }
        }

        public string SaveVote(VoteSaveModel voteModel)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string sql = "INSERT INTO vote (election_id, candidate_id, vote_date, voter_id) " +
                                 "VALUES (@election_id, @candidate_id, @vote_date, @voter_id);";

                    var parameters = new
                    {
                        election_id = voteModel.election_id,
                        candidate_id = voteModel.candidate_id,
                        vote_date = voteModel.vote_date ?? DateTime.Now, // Use current timestamp if not provided
                        voter_id = voteModel.voter_id
                    };

                    int rowsAffected = dbConnection.Execute(sql, parameters);

                    return rowsAffected > 0 ? "Success" : "Failure";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while saving the vote.", ex);
            }
        }


        public string SaveAnnouncement(AnnouncementSaveModel announcementModel)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string sql = "INSERT INTO announcement (announcement_by_id, club_id, user_id, created_at, announcement_text, announcement_title) " +
                                 "VALUES (@announcement_by_id, @club_id, @user_id, @created_at, @announcement_text, @announcement_title);";

                    var parameters = new
                    {
                        announcement_by_id = announcementModel.announcement_by_id,
                        club_id = announcementModel.club_id ?? (object)DBNull.Value,  
                        user_id = announcementModel.user_id ?? (object)DBNull.Value, 
                        created_at = announcementModel.created_at,
                        announcement_text = announcementModel.announcement_text,
                        announcement_title = announcementModel.announcement_title
                    };

                    int rowsAffected = dbConnection.Execute(sql, parameters);

                    return rowsAffected > 0 ? "Success" : "Failure";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while saving the announcement.", ex);
            }
        }

        public string RegisterClubMember(string userId, int clubId)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();

                    string checkMembershipQuery = @"SELECT COUNT(*) FROM club_members WHERE user_id = @UserId AND club_id = @ClubId;";
                    int count = dbConnection.QuerySingle<int>(checkMembershipQuery, new { UserId = userId, ClubId = clubId });

                    if (count > 0)
                    {
                        return "User is already registered in this club.";
                    }

                    string insertQuery = @"INSERT INTO club_members (user_id, club_id, status, joined_at)
                                   VALUES (@UserId, @ClubId, 'approved', NOW());";
                    int rowsInserted = dbConnection.Execute(insertQuery, new { UserId = userId, ClubId = clubId });

                    if (rowsInserted <= 0)
                    {
                        return "Failed to add member to club.";
                    }

                    string selectQuery = "SELECT membership_id FROM club_members WHERE user_id = @UserId AND club_id = @ClubId ORDER BY membership_id DESC LIMIT 1";
                    int membershipId = dbConnection.QuerySingle<int>(selectQuery, new { UserId = userId, ClubId = clubId });

                    string updateUserQuery = @"UPDATE user 
                                       SET membership_id = @MembershipId, role_id = 4 
                                       WHERE user_id = @UserId;";
                    int rowsUpdated = dbConnection.Execute(updateUserQuery, new { MembershipId = membershipId, UserId = userId });

                    return rowsUpdated > 0 ? "Success" : "Failed to update user role and membership.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while registering the club member. UserId: {userId}, ClubId: {clubId}.", ex);
            }
        }

        public string UpdateUserRole(string userId, string action)
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();

                    string query = string.Empty;
                    object parameters;

                    switch (action.ToLower())
                    {
                        case "promote":
                            query = "UPDATE user SET role_id = role_id - 1 WHERE user_id = @UserId";
                            parameters = new { UserId = userId };
                            break;

                        case "demote":
                            query = "UPDATE user SET role_id = role_id + 1 WHERE user_id = @UserId";
                            parameters = new { UserId = userId };
                            break;

                        case "remove":
                            query = "UPDATE user SET role_id = 5, membership_id = NULL WHERE user_id = @UserId";
                            int rowsUpdated = dbConnection.Execute(query, new { UserId = userId });

                            if (rowsUpdated > 0)
                            {
                                query = "DELETE FROM club_members WHERE user_id = @UserId";
                                dbConnection.Execute(query, new { UserId = userId });
                            }

                            return rowsUpdated > 0 ? "Success" : "User not found or no changes made.";

                        default:
                            return "Invalid action. Must be 'Promote', 'Demote', or 'Remove'.";
                    }

                    int rowsAffected = dbConnection.Execute(query, parameters);

                    return rowsAffected > 0 ? "Success" : "User not found or no changes made.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating user role. Action: {action}, UserId: {userId}.", ex);
            }
        }

        public string UpdateEventStatus(int eventId, string action)
        {
            try
            {
                if (action != "Approve" && action != "Reject")
                    return "Invalid action. Must be 'approve' or 'reject'.";

                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string sql = "UPDATE events SET approval = @Approval WHERE event_id = @EventId";

                    var parameters = new
                    {
                        Approval = action.Equals("approve", StringComparison.OrdinalIgnoreCase) ? "approved" : "rejected",
                        EventId = eventId
                    };

                    int rowsAffected = dbConnection.Execute(sql, parameters);

                    return rowsAffected > 0 ? "Success" : "Event not found or no changes made.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while updating event status.", ex);
            }
        }


        public async Task<List<Message>> GetMessagesByClubIdAsync(int clubId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sqlMessages = @"
            SELECT 
                message_id AS MessageId, 
                user_id AS UserId, 
                club_id AS ClubId, 
                message AS MessageText, 
                timestamp AS Timestamp 
            FROM 
                messages 
            WHERE 
                club_id = @ClubId";

                string sqlUsers = @"
            SELECT 
                user_id AS UserId 
            FROM 
                user";

                var messages = (await connection.QueryAsync<Message>(sqlMessages, new { ClubId = clubId })).ToList();

                if (messages.Any())
                {
                    var users = (await connection.QueryAsync<dynamic>(sqlUsers)).ToList();
                }

                return messages;
            }
        }

        public async Task<int> AddMessageAsync(Message message)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"INSERT INTO messages (user_id, club_id, message, timestamp)
                             VALUES (@UserId, @ClubId, @MessageText, @Timestamp);
                             SELECT LAST_INSERT_ID();";

                return await connection.ExecuteScalarAsync<int>(query, message);
            }
        }

        public string GetLatestElectionStatus()
        {
            try
            {
                using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    string sql = "SELECT * FROM election ORDER BY election_id DESC LIMIT 1;";
                    var latestElection = dbConnection.QueryFirstOrDefault<Election>(sql);

                    if (latestElection == null)
                    {
                        return "No Elections";
                    }

                    DateTime today = DateTime.Today;

                    if (latestElection.start_date > today && latestElection.status == "voting")
                    {
                        return "Election Upcoming";
                    }
                    else if (latestElection.start_date <= today && latestElection.end_date >= today && latestElection.status == "voting")
                    {
                        return "Show Candidate";
                    }
                    else if (latestElection.end_date < today && latestElection.status == "voting")
                    {
                        return "Processing Election";
                    }
                    else if (latestElection.end_date < today && latestElection.status == "elected")
                    {
                        return "Show Results";
                    }
                    else
                    {
                        return "Unknown Status";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving the latest election status.", ex);
            }
        }
    }
}
