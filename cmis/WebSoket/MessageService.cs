using cmis.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace cmis.WebSoket
{
    public interface IMessageService
    {
        Task SaveMessageAsync(MessageModel message);
        Task<List<MessageModel>> GetMessagesByClubIdAsync(int clubId);
    }
    public class MessageService: IMessageService
    {
        private readonly string _connectionString;
        public MessageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySQLConnection");
        }
        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task SaveMessageAsync(MessageModel message)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "INSERT INTO messages (user_id, club_id, message, timestamp) VALUES (@UserId, @ClubId, @Message, @Timestamp)";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", message.user_id);
            command.Parameters.AddWithValue("@ClubId", message.club_id);
            command.Parameters.AddWithValue("@Message", message.message);
            command.Parameters.AddWithValue("@Timestamp", message.timestamp);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<MessageModel>> GetMessagesByClubIdAsync(int clubId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT user_id, club_id, message, timestamp FROM messages WHERE club_id = @ClubId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClubId", clubId);

            using var reader = await command.ExecuteReaderAsync();
            var messages = new List<MessageModel>();

            while (await reader.ReadAsync())
            {
                messages.Add(new MessageModel
                {
                    user_id = reader.GetString("user_id"),
                    club_id = reader.GetInt32("club_id"),
                    message = reader.GetString("message"),
                    timestamp = reader.GetDateTime("timestamp")
                });
            }
            return messages;
        }
    }
}

