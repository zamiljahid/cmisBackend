using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using cmis.Model;
using Dapper;

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

        //public async Task<List<ProfileModel>> GetAllItemsAsync()
        //{
        //    using (var connection = CreateConnection())
        //    {
        //        connection.Open();
        //        var result = await connection.QueryAsync<ProfileModel>("SELECT * FROM user");
        //        return result.ToList();
        //    }
        //}

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

    }
}
