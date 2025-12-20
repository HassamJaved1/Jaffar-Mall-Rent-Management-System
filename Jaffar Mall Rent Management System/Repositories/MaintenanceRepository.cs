using Dapper;
using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Repositories
{
    public class MaintenanceRepository
    {
        private readonly string _connectionString;

        public MaintenanceRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<long> AddMaintenanceAsync(Maintenance item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                INSERT INTO maintenance
                  (tenant_id, property_id, title, description, status, priority, assigned_to, created_at, updated_at, resolved_at)
                VALUES
                  (@TenantId, @PropertyId, @Title, @Description, @Status, @Priority, @AssignedTo, @CreatedAt, @UpdatedAt, @ResolvedAt)
                RETURNING id";

                var parameters = new
                {
                    TenantId = item.TenantId,
                    PropertyId = item.PropertyId,
                    Title = item.Title,
                    Description = item.Description,
                    Status = (short)item.Status,
                    Priority = (short)item.Priority,
                    AssignedTo = item.AssignedTo,
                    ResolvedAt = item.ResolvedAt
                };

                long id = await connection.ExecuteScalarAsync<long>(sql, parameters);
                if (id > 0) item.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public async Task<IEnumerable<Maintenance>> GetAllAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                  id,
                  tenant_id AS ""TenantId"",
                  property_id AS ""PropertyId"",
                  title,
                  description,
                  status AS ""Status"",
                  priority AS ""Priority"",
                  assigned_to AS ""AssignedTo"",
                  created_at AS ""CreatedAt"",
                  updated_at AS ""UpdatedAt"",
                  resolved_at AS ""ResolvedAt""
                FROM maintenance
                ORDER BY created_at DESC";

                return await connection.QueryAsync<Maintenance>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Maintenance>();
            }
        }
        
        public async Task<Maintenance?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                  id,
                  tenant_id AS ""TenantId"",
                  property_id AS ""PropertyId"",
                  title,
                  description,
                  status AS ""Status"",
                  priority AS ""Priority"",
                  assigned_to AS ""AssignedTo"",
                  created_at AS ""CreatedAt"",
                  updated_at AS ""UpdatedAt"",
                  resolved_at AS ""ResolvedAt""
                FROM maintenance
                WHERE id = @Id";

                return await connection.QuerySingleOrDefaultAsync<Maintenance?>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<Maintenance>> GetByTenantIdAsync(long tenantId)
        {
            if (tenantId <= 0) return Array.Empty<Maintenance>();

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                  id,
                  tenant_id AS ""TenantId"",
                  property_id AS ""PropertyId"",
                  title,
                  description,
                  status AS ""Status"",
                  priority AS ""Priority"",
                  assigned_to AS ""AssignedTo"",
                  created_at AS ""CreatedAt"",
                  updated_at AS ""UpdatedAt"",
                  resolved_at AS ""ResolvedAt""
                FROM maintenance
                WHERE tenant_id = @TenantId
                ORDER BY created_at DESC";

                return await connection.QueryAsync<Maintenance>(sql, new { TenantId = tenantId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Maintenance>();
            }
        }

        public async Task<IEnumerable<Maintenance>> GetByPropertyIdAsync(long propertyId)
        {
            if (propertyId <= 0) return Array.Empty<Maintenance>();

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                  id,
                  tenant_id AS ""TenantId"",
                  property_id AS ""PropertyId"",
                  title,
                  description,
                  status AS ""Status"",
                  priority AS ""Priority"",
                  assigned_to AS ""AssignedTo"",
                  created_at AS ""CreatedAt"",
                  updated_at AS ""UpdatedAt"",
                  resolved_at AS ""ResolvedAt""
                FROM maintenance
                WHERE property_id = @PropertyId
                ORDER BY created_at DESC";

                return await connection.QueryAsync<Maintenance>(sql, new { PropertyId = propertyId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<Maintenance>();
            }
        }

        public async Task<bool> UpdateAsync(Maintenance item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Id <= 0) throw new ArgumentException("Maintenance item must have a valid Id to update", nameof(item));

            try
            {
                item.UpdatedAt = DateTime.UtcNow;

                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                UPDATE maintenance
                SET title = @Title,
                    description = @Description,
                    status = @Status,
                    priority = @Priority,
                    assigned_to = @AssignedTo,
                    resolved_at = @ResolvedAt
                WHERE id = @Id";

                var parameters = new
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Status = (short)item.Status,
                    Priority = (short)item.Priority,
                    AssignedTo = item.AssignedTo,
                    ResolvedAt = item.ResolvedAt
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

        public async Task<bool> DeleteAsync(long id)
        {
            if (id <= 0) return false;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                DELETE FROM maintenance WHERE id = @Id";

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