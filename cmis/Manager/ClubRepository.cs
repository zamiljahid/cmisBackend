using cmis.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
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
                AND e.approval = 'approved'"; // Only include approved events

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
            GROUP_CONCAT(DISTINCT cm.club_id) AS ClubsInvolved
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
            r.role_id = 2 
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
    }
}
