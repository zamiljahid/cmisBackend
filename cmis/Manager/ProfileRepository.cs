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
    public class ProfileRepository : IProfileRepository
    {
        private readonly string _connectionString;

        public ProfileRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySQLConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public ProfileModel GetUserProfileById(string user_id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
SELECT 
    u.user_id AS UserID, 
    u.name AS Name, 
    u.email AS Email, 
    r.role_name AS Role,
    u.role_id AS RoleID,
    u.contact AS Contact, 
    u.profile_pic_url AS ProfilePicUrl,
    m.membership_id AS MembershipId, 
    c.club_id AS ClubId, 
    c.club_name AS ClubName
FROM 
    user u
JOIN 
    role r ON u.role_id = r.role_id
LEFT JOIN 
    club_members m ON u.user_id = m.user_id
LEFT JOIN 
    club c ON m.club_id = c.club_id
WHERE 
    u.user_id = @user_id";

                return connection.QueryFirstOrDefault<ProfileModel>(sql, new { user_id });
            }
        }

        public async Task<List<AnnouncementModel>> GetAnnouncementsAsync(int? club_id, string user_id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                string sql = @"
SELECT 
    announcement_id AS AnnouncementId,
    announcement_by_id AS AnnouncementById,
    club_id AS ClubId,
    user_id AS UserId,
    created_at AS CreatedAt,
    announcement_text AS AnnouncementText,
    announcement_title AS AnnouncementTitle
FROM 
    announcement
WHERE 
    (@club_id IS NOT NULL AND club_id = @club_id)
    OR (@user_id IS NOT NULL AND user_id = @user_id)";


                var parameters = new { club_id, user_id };
                var result = await connection.QueryAsync<AnnouncementModel>(sql, parameters);

                return result.ToList();
            }
        }
    }
}
