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

                // Schema Migration for new columns
                try {
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS maintenance (
                            id BIGSERIAL PRIMARY KEY,
                            tenant_id BIGINT,
                            property_id BIGINT NOT NULL,
                            title VARCHAR(255) NOT NULL,
                            description TEXT,
                            status SMALLINT NOT NULL,
                            priority SMALLINT NOT NULL,
                            assigned_to VARCHAR(255),
                            created_at TIMESTAMP NOT NULL,
                            updated_at TIMESTAMP NOT NULL,
                            resolved_at TIMESTAMP
                        );
                    ");
                    await connection.ExecuteAsync("ALTER TABLE maintenance ADD COLUMN IF NOT EXISTS repair_cost DECIMAL(18,2) DEFAULT 0;");
                    await connection.ExecuteAsync("ALTER TABLE maintenance ADD COLUMN IF NOT EXISTS amount_paid DECIMAL(18,2) DEFAULT 0;");
                    await connection.ExecuteAsync("ALTER TABLE maintenance ADD COLUMN IF NOT EXISTS repairer_name VARCHAR(255);");
                    await connection.ExecuteAsync("ALTER TABLE maintenance ADD COLUMN IF NOT EXISTS repairer_details TEXT;");
                } catch { }

                const string sql = @"
                INSERT INTO maintenance
                  (tenant_id, property_id, title, description, status, priority, assigned_to, created_at, updated_at, resolved_at, repair_cost, amount_paid, repairer_name, repairer_details)
                VALUES
                  (@TenantId, @PropertyId, @Title, @Description, @Status, @Priority, @AssignedTo, @CreatedAt, @UpdatedAt, @ResolvedAt, @RepairCost, @AmountPaid, @RepairerName, @RepairerDetails)
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ResolvedAt = item.ResolvedAt,
                    RepairCost = item.RepairCost,
                    AmountPaid = item.AmountPaid,
                    RepairerName = item.RepairerName,
                    RepairerDetails = item.RepairerDetails
                };

                long id = await connection.ExecuteScalarAsync<long>(sql, parameters);
                if (id > 0) item.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
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
                  resolved_at AS ""ResolvedAt"",
                  repair_cost AS ""RepairCost"",
                  amount_paid AS ""AmountPaid"",
                  repairer_name AS ""RepairerName"",
                  repairer_details AS ""RepairerDetails""
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
                  resolved_at AS ""ResolvedAt"",
                  repair_cost AS ""RepairCost"",
                  amount_paid AS ""AmountPaid"",
                  repairer_name AS ""RepairerName"",
                  repairer_details AS ""RepairerDetails""
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
                  resolved_at AS ""ResolvedAt"",
                  repair_cost AS ""RepairCost"",
                  amount_paid AS ""AmountPaid"",
                  repairer_name AS ""RepairerName"",
                  repairer_details AS ""RepairerDetails""
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
                  resolved_at AS ""ResolvedAt"",
                  repair_cost AS ""RepairCost"",
                  amount_paid AS ""AmountPaid"",
                  repairer_name AS ""RepairerName"",
                  repairer_details AS ""RepairerDetails""
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
                    resolved_at = @ResolvedAt,
                    repair_cost = @RepairCost,
                    amount_paid = @AmountPaid,
                    repairer_name = @RepairerName,
                    repairer_details = @RepairerDetails,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

                var parameters = new
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Status = (short)item.Status,
                    Priority = (short)item.Priority,
                    AssignedTo = item.AssignedTo,
                    ResolvedAt = item.ResolvedAt,
                    RepairCost = item.RepairCost,
                    AmountPaid = item.AmountPaid,
                    RepairerName = item.RepairerName,
                    RepairerDetails = item.RepairerDetails,
                    UpdatedAt = item.UpdatedAt
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