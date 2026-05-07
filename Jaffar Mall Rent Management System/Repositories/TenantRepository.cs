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

        public async Task<int> GetTotalTenantsCountAsync(string? searchTerm = null)
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"
                SELECT COUNT(*) 
                FROM tenants
                WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " AND (name ILIKE @SearchTerm OR phone_no ILIKE @SearchTerm OR email ILIKE @SearchTerm OR card_number ILIKE @SearchTerm)";
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

                try {
                    await connection.ExecuteAsync("ALTER TABLE tenants ADD COLUMN IF NOT EXISTS email VARCHAR(255);");
                    await connection.ExecuteAsync("ALTER TABLE tenants ADD COLUMN IF NOT EXISTS id_card_image_url VARCHAR(500);");
                } catch { }

                const string sql = @"
                INSERT INTO tenants
                    (name, description, phone_no, email, card_number, address, city, country, id_card_image_url, created_at, updated_at)
                VALUES
                    (@Name, @Description, @Phone_No, @Email, @CardNumber, @Address, @City, @Country, @IdCardImageUrl, @CreatedAt, @UpdatedAt)
                RETURNING id";

                var parameters = new
                {
                    Name = tenant.Name,
                    Description = tenant.Description,
                    Phone_No = tenant.Phone_No,
                    Email = tenant.Email,
                    CardNumber = tenant.CardNumber,
                    Address = tenant.Address,
                    City = tenant.City,
                    Country = tenant.Country,
                    IdCardImageUrl = tenant.IdCardImageUrl,
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

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(int skip, int take, string? searchTerm = null, string? sort = null)
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                SELECT
                    id,
                    name,
                    description,
                    phone_no AS ""Phone_No"",
                    email AS ""Email"",
                    card_number AS ""CardNumber"",
                    address AS ""Address"",
                    city AS ""City"",
                    country AS ""Country"",
                    id_card_image_url AS ""IdCardImageUrl"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM tenants
                WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " AND (name ILIKE @SearchTerm OR phone_no ILIKE @SearchTerm OR email ILIKE @SearchTerm OR card_number ILIKE @SearchTerm)";
                }

                string sortClause = "ORDER BY id";
                if (sort == "name_asc") sortClause = "ORDER BY name ASC";
                else if (sort == "name_desc") sortClause = "ORDER BY name DESC";
                else if (sort == "date_desc") sortClause = "ORDER BY created_at DESC";
                else if (sort == "date_asc") sortClause = "ORDER BY created_at ASC";

                sql += $"\n{sortClause}\nOFFSET @Skip LIMIT @Take";

                var tenants = await connection.QueryAsync<Tenant>(sql, new 
                { 
                    Skip = skip, 
                    Take = take,
                    SearchTerm = $"%{searchTerm}%"
                });
                return tenants;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Tenant>();
            }
        }

        // Overload for getting all tenants (used by dropdowns)
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
                    email AS ""Email"",
                    card_number AS ""CardNumber"",
                    address AS ""Address"",
                    city AS ""City"",
                    country AS ""Country"",
                    id_card_image_url AS ""IdCardImageUrl"",
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
                    email AS ""Email"",
                    card_number AS ""CardNumber"",
                    address AS ""Address"",
                    city AS ""City"",
                    country AS ""Country"",
                    id_card_image_url AS ""IdCardImageUrl"",
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
                    email = @Email,
                    card_number = @CardNumber,
                    address = @Address,
                    city = @City,
                    country = @Country,
                    id_card_image_url = @IdCardImageUrl,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

                var parameters = new
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Description = tenant.Description,
                    Phone_No = tenant.Phone_No,
                    Email = tenant.Email,
                    CardNumber = tenant.CardNumber,
                    Address = tenant.Address,
                    City = tenant.City,
                    Country = tenant.Country,
                    IdCardImageUrl = tenant.IdCardImageUrl,
                    UpdatedAt = tenant.UpdatedAt
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
