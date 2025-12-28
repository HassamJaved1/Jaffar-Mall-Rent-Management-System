using Dapper;
using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Repositories
{
    public class TenantRepository
    {
        private readonly string _connectionString;

        public TenantRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<int> GetTotalTenantsCountAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                const string sql = @"SELECT COUNT(*) FROM tenants";
                int count = await connection.ExecuteScalarAsync<int>(sql);
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public async Task<bool> AddTenantAsync(Tenant tenant)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));

            try
            {
                var now = DateTime.UtcNow;
                tenant.CreatedAt = now;
                tenant.UpdatedAt = now;

                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                INSERT INTO tenants
                    (name, description, phone_no, card_number, address, city, country, created_at, updated_at)
                VALUES
                    (@Name, @Description, @Phone_No, @CardNumber, @Address, @City, @Country, @CreatedAt, @UpdatedAt)
                RETURNING id";

                var parameters = new
                {
                    Name = tenant.Name,
                    Description = tenant.Description,
                    Phone_No = tenant.Phone_No,
                    CardNumber = tenant.CardNumber,
                    Address = tenant.Address,
                    City = tenant.City,
                    Country = tenant.Country,
                    CreatedAt = tenant.CreatedAt,
                    UpdatedAt = tenant.UpdatedAt
                };

                long insertedId = await connection.ExecuteScalarAsync<long>(sql, parameters);

                if (insertedId > 0)
                {
                    tenant.Id = insertedId;
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

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    id,
                    name,
                    description,
                    phone_no AS ""Phone_No"",
                    card_number AS ""CardNumber"",
                    address AS ""Address"",
                    city AS ""City"",
                    country AS ""Country"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM tenants
                ORDER BY id";

                var tenants = await connection.QueryAsync<Tenant>(sql);
                return tenants;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Tenant>();
            }
        }

        public async Task<Tenant?> GetTenantByIdAsync(long id)
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
                    description,
                    phone_no AS ""Phone_No"",
                    card_number AS ""CardNumber"",
                    address AS ""Address"",
                    city AS ""City"",
                    country AS ""Country"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM tenants
                WHERE id = @Id";

                var tenant = await connection.QuerySingleOrDefaultAsync<Tenant?>(sql, new { Id = id });
                return tenant;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateTenantAsync(Tenant tenant)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            if (tenant.Id <= 0) throw new ArgumentException("Tenant must have a valid Id to update.", nameof(tenant));

            try
            {
                var now = DateTime.UtcNow;
                tenant.UpdatedAt = now;

                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                UPDATE tenants
                SET
                    name = @Name,
                    description = @Description,
                    phone_no = @Phone_No,
                    card_number = @CardNumber,
                    address = @Address,
                    city = @City,
                    country = @Country
                WHERE id = @Id";

                var parameters = new
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Description = tenant.Description,
                    Phone_No = tenant.Phone_No,
                    CardNumber = tenant.CardNumber,
                    Address = tenant.Address,
                    City = tenant.City,
                    Country = tenant.Country
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

        public async Task<bool> DeleteTenantAsync(long id)
        {
            if (id <= 0) return false;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                DELETE FROM tenants
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
