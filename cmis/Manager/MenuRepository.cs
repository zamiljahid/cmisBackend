using cmis.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace cmis.Manager
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;

        public MenuRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySQLConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public List<MenuModel> GetMenusByRoleId(string role_id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                string sql = @"
                    SELECT 
                        m.menu_id, 
                        m.menu_name 
                    FROM 
                        role_menu rm
                    JOIN 
                        menu m ON rm.menu_id = m.menu_id
                    WHERE 
                        rm.role_id = @role_id";

                return connection.Query<MenuModel>(sql, new { role_id }).AsList();
            }
        }
    }
}
