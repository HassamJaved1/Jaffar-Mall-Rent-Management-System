using Dapper;
using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Repositories
{
    public class PropertyRepository
    {
        private readonly string _connectionString;

        public PropertyRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<int> GetTotalPropertiesCountAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                const string sql = @"
                SELECT COUNT(*) 
                FROM properties";
                int count = await connection.ExecuteScalarAsync<int>(sql);
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public async Task<bool> AddPropertyAsync(Property property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                INSERT INTO properties (name, property_type,property_code,description, status)
                VALUES (@Name, @PropertyType,@PropertyCode,@Description, @Status)
                RETURNING id";

                var parameters = new
                {
                    Name = property.Name,
                    PropertyType = property.PropertyType,
                    PropertyCode = property.PropertyCode,
                    Description = property.Description, 
                    Status = (int)property.Status
                };

                long insertedId = await connection.ExecuteScalarAsync<long>(sql, parameters);

                if (insertedId > 0)
                {
                    property.Id = insertedId;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdatePropertyAsync(Property property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (property.Id <= 0) throw new ArgumentException("Property must have a valid Id to update.", nameof(property));

            try
            {
                var now = DateTime.UtcNow;
                property.UpdatedAt = now;

                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                UPDATE properties
                SET name = @Name,
                    property_type = @PropertyType,
                    property_code = @PropertyCode,
                    description = @Description,
                    status = @Status,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

                var parameters = new
                {
                    Id = property.Id,
                    Name = property.Name,
                    PropertyType = property.PropertyType,
                    PropertyCode = property.PropertyCode,
                    Description = property.Description,
                    Status = (int)property.Status,
                    UpdatedAt = property.UpdatedAt
                };

                int rows = await connection.ExecuteAsync(sql, parameters);
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    id,
                    name,
                    property_type AS ""PropertyType"",
                    property_code AS ""PropertyCode"",
                    status AS ""Status"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM properties
                ORDER BY id";

                var properties = await connection.QueryAsync<Property>(sql);
                return properties;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Property>();
            }
        }

        public async Task<Property?> GetPropertyByIdAsync(long id)
        {
            if (id <= 0) return null;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    id,
                    name,
                    property_type AS ""PropertyType"",
                    property_code AS ""PropertyCode"",
                    status AS ""Status"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM properties
                WHERE id = @Id";

                var property = await connection.QuerySingleOrDefaultAsync<Property?>(sql, new { Id = id });
                return property;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> DeletePropertyAsync(long id)
        {
            if (id <= 0) return false;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                DELETE FROM properties
                WHERE id = @Id";

                int rows = await connection.ExecuteAsync(sql, new { Id = id });
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
