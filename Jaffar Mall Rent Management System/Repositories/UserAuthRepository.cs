
using Dapper;
using Jaffar_Mall_Rent_Management_System.Models;
using Npgsql;

namespace Jaffar_Mall_Rent_Management_System.Backend_Logics
{
    public class UserAuthRepository
    {
        private readonly string _connectionString;

        public UserAuthRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<User?> GetDetailsByUsernameAsync(string username)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT id, password
                FROM users
                WHERE username = @Username";

                var userDetails = await connection.QuerySingleOrDefaultAsync<User?>(
                    sql,
                    new { Username = username }
                );

                return userDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<int> UpdatePasswordAsync(long userId, string hashedPassword)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sqlUpdate = @"
                UPDATE users
                SET password = @PasswordHash
                WHERE id = @UserId";

                int rowsAffected = await connection.ExecuteAsync(sqlUpdate, new { PasswordHash = hashedPassword, UserId = userId });
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Log the exception when needed
                return 0;
            }
        }
    }

}
