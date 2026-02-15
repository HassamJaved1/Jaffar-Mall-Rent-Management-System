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

        public async Task<int> GetTotalPropertiesCountAsync(string? searchTerm = null)
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"
                SELECT COUNT(*) 
                FROM properties
                WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " AND (name ILIKE @SearchTerm OR address ILIKE @SearchTerm OR city ILIKE @SearchTerm OR property_number ILIKE @SearchTerm)";
                }

                int count = await connection.ExecuteScalarAsync<int>(sql, new { SearchTerm = $"%{searchTerm}%" });
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
                INSERT INTO properties
                    (name, description, property_type, property_code, property_number, address, city, country)
                VALUES
                    (@Name, @Description, @PropertyType, @PropertyCode, @PropertyNumber, @Address, @City, @Country)
                RETURNING id";

                var parameters = new
                {
                    Name = property.Name,
                    Description = property.Description,
                    PropertyType = property.PropertyType,
                    PropertyCode = Guid.NewGuid(),
                    PropertyNumber = property.PropertyNumber,
                    Address = property.Address,
                    City = property.City,
                    Country = property.Country
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
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                UPDATE properties
                SET name = @Name,
                    description = @Description,
                    property_type = @PropertyType,
                    property_number = @PropertyNumber,
                    address = @Address,
                    city = @City,
                    country = @Country,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

                var parameters = new
                {
                    Id = property.Id,
                    Name = property.Name,
                    Description = property.Description,
                    PropertyType = property.PropertyType,
                    PropertyNumber = property.PropertyNumber,
                    Address = property.Address,
                    City = property.City,
                    Country = property.Country,
                    UpdatedAt = DateTime.UtcNow
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

        public async Task<IEnumerable<Property>> GetAllPropertiesAsync(int skip, int take, string? searchTerm = null)
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                SELECT
                    p.id,
                    p.name,
                    p.description,
                    p.property_type AS ""PropertyType"",
                    p.property_code AS ""PropertyCode"",
                    p.property_number AS ""PropertyNumber"",
                    p.status AS ""Status"",
                    p.address AS ""Address"",
                    p.city AS ""City"",
                    p.country AS ""Country"",
                    p.created_at AS ""CreatedAt"",
                    p.updated_at AS ""UpdatedAt"",
                    t.name AS ""TenantName""
                FROM properties p
                LEFT JOIN property_leases pl ON p.id = pl.property_id AND pl.status = 2
                LEFT JOIN tenants t ON pl.tenant_id = t.id
                WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " AND (p.name ILIKE @SearchTerm OR p.address ILIKE @SearchTerm OR p.city ILIKE @SearchTerm OR p.property_number ILIKE @SearchTerm)";
                }

                sql += @"
                ORDER BY p.id
                OFFSET @Skip LIMIT @Take";

                var properties = await connection.QueryAsync<Property>(sql, new 
                { 
                    Skip = skip, 
                    Take = take,
                    SearchTerm = $"%{searchTerm}%"
                });
                return properties;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Property>();
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
                    p.id,
                    p.name,
                    p.description,
                    p.property_type AS ""PropertyType"",
                    p.property_code AS ""PropertyCode"",
                    p.property_number AS ""PropertyNumber"",
                    p.status AS ""Status"",
                    p.address AS ""Address"",
                    p.city AS ""City"",
                    p.country AS ""Country"",
                    p.created_at AS ""CreatedAt"",
                    p.updated_at AS ""UpdatedAt"",
                    t.name AS ""TenantName""
                FROM properties p
                LEFT JOIN property_leases pl ON p.id = pl.property_id AND pl.status = 2
                LEFT JOIN tenants t ON pl.tenant_id = t.id
                ORDER BY p.id";

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
                    p.id,
                    p.name,
                    p.description,
                    p.property_type AS ""PropertyType"",
                    p.property_code AS ""PropertyCode"",
                    p.property_number AS ""PropertyNumber"",
                    p.status AS ""Status"",
                    p.address AS ""Address"",
                    p.city AS ""City"",
                    p.country AS ""Country"",
                    p.created_at AS ""CreatedAt"",
                    p.updated_at AS ""UpdatedAt"",
                    t.name AS ""TenantName""
                FROM properties p
                LEFT JOIN property_leases pl ON p.id = pl.property_id AND pl.status = 2
                LEFT JOIN tenants t ON pl.tenant_id = t.id
                WHERE p.id = @Id";

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
        public async Task<IEnumerable<Property>> GetVacantPropertiesAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    p.id,
                    p.name,
                    p.description,
                    p.property_type AS ""PropertyType"",
                    p.property_code AS ""PropertyCode"",
                    p.property_number AS ""PropertyNumber"",
                    p.status AS ""Status"",
                    p.address AS ""Address"",
                    p.city AS ""City"",
                    p.country AS ""Country"",
                    p.created_at AS ""CreatedAt"",
                    p.updated_at AS ""UpdatedAt""
                FROM properties p
                LEFT JOIN property_leases pl ON p.id = pl.property_id AND pl.status = 2
                WHERE pl.id IS NULL
                ORDER BY p.name";

                return await connection.QueryAsync<Property>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Property>();
            }
        }
    }
}
