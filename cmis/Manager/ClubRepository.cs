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
    u.email AS Email
FROM 
    user u
JOIN 
    role r ON u.role_id = r.role_id  
WHERE 
    u.membership_id IN @MembershipIds";

                var members = (await connection.QueryAsync<MembershipModel>(sqlMembers, new { MembershipIds = membershipIds })).ToList();

                return members;
            }
        }


    }
}
