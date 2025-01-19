using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Xml;
using cmis.Model;
using Dapper;
namespace cmis.Manager
{
    public class LoginRepository: ILoginRepository
    {
        private readonly string _connectionString;

        public LoginRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySQLConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public LoginModel GetLoginById(string user_id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = "select user_id, password from user where user_id = @user_id";

                var login = connection.QueryFirstOrDefault<LoginModel>(sql, new { user_id = user_id });

                return login;  // Returns null if no record found
            }
        }

        public async Task<List<LoginModel>> GetAllItemsAsync()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var result = await connection.QueryAsync<LoginModel>("SELECT * FROM MyTable");
                return result.ToList();
            }
        }
    }
}
