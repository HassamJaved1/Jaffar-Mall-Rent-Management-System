using Dapper;
using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Repositories
{
    public class LeasesRepository
    {
        private readonly string _connectionString;

        public LeasesRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<int> GetTotalLeasesCountAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"SELECT COUNT(*) FROM property_leases";
                int count = await connection.ExecuteScalarAsync<int>(sql);
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public async Task<bool> AddLeaseAsync(PropertyLease lease)
        {
            if (lease == null) throw new ArgumentNullException(nameof(lease));

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                INSERT INTO property_leases
                    (tenant_id, property_id, description, status, rent_amount,months, security_deposit, added_by)
                VALUES
                    (@TenantId, @PropertyId, @Description, @Status, @RentAmount,@Months, @SecurityDeposit, @AddedBy)
                RETURNING id";

                var parameters = new
                {
                    TenantId = lease.TenantId,
                    PropertyId = lease.PropertyId,
                    Description = lease.Description,
                    Status = (short)lease.Status,
                    RentAmount = lease.RentAmount,
                    SecurityDeposit = lease.SecurityDeposit,
                    Months = lease.Months,
                    AddedBy = lease.AddedBy
                };

                long insertedId = await connection.ExecuteScalarAsync<long>(sql, parameters);

                if (insertedId > 0)
                {
                    lease.Id = insertedId;
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

        public async Task<IEnumerable<PropertyLease>> GetAllLeasesAsync()
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
                    description,
                    status AS ""Status"",
                    rent_amount AS ""RentAmount"",
                    months AS ""Months"",
                    security_deposit AS ""SecurityDeposit"",
                    added_by AS ""AddedBy"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM property_leases
                ORDER BY created_at DESC";

                var leases = await connection.QueryAsync<PropertyLease>(sql);
                return leases;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<PropertyLease>();
            }
        }

        public async Task<PropertyLease?> GetLeaseByIdAsync(long id)
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
                    description,
                    status AS ""Status"",
                    months AS ""Months"",
                    rent_amount AS ""RentAmount"",
                    security_deposit AS ""SecurityDeposit"",
                    added_by AS ""AddedBy"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM property_leases
                WHERE id = @Id";

                var lease = await connection.QuerySingleOrDefaultAsync<PropertyLease?>(sql, new { Id = id });
                return lease;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<PropertyLease>> GetLeasesByTenantIdAsync(long tenantId)
        {
            if (tenantId <= 0) return Array.Empty<PropertyLease>();

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    id,
                    tenant_id AS ""TenantId"",
                    property_id AS ""PropertyId"",
                    description,
                    status AS ""Status"",
                    rent_amount AS ""RentAmount"",
                    security_deposit AS ""SecurityDeposit"",
                    added_by AS ""AddedBy"",
                    months AS ""Months"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM property_leases
                WHERE tenant_id = @TenantId
                ORDER BY created_at DESC";

                return await connection.QueryAsync<PropertyLease>(sql, new { TenantId = tenantId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<PropertyLease>();
            }
        }

        public async Task<IEnumerable<PropertyLease>> GetLeasesByPropertyIdAsync(long propertyId)
        {
            if (propertyId <= 0) return Array.Empty<PropertyLease>();

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    id,
                    tenant_id AS ""TenantId"",
                    property_id AS ""PropertyId"",
                    description,
                    status AS ""Status"",
                    rent_amount AS ""RentAmount"",
                    security_deposit AS ""SecurityDeposit"",
                    added_by AS ""AddedBy"",
                    months AS ""Months"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM property_leases
                WHERE property_id = @PropertyId
                ORDER BY created_at DESC";

                return await connection.QueryAsync<PropertyLease>(sql, new { PropertyId = propertyId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<PropertyLease>();
            }
        }

        public async Task<bool> UpdateLeaseAsync(PropertyLease lease)
        {
            if (lease == null) throw new ArgumentNullException(nameof(lease));
            if (lease.Id <= 0) throw new ArgumentException("Lease must have a valid Id to update.", nameof(lease));

            try
            {
                lease.UpdatedAt = DateTime.UtcNow;

                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                UPDATE property_leases
                SET tenant_id = @TenantId,
                    property_id = @PropertyId,
                    description = @Description,
                    status = @Status,
                    months = @Months,
                    rent_amount = @RentAmount,
                    security_deposit = @SecurityDeposit,
                    added_by = @AddedBy,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

                var parameters = new
                {
                    Id = lease.Id,
                    TenantId = lease.TenantId,
                    PropertyId = lease.PropertyId,
                    Description = lease.Description,
                    Status = (short)lease.Status,
                    RentAmount = lease.RentAmount,
                    Months = lease.Months,
                    SecurityDeposit = lease.SecurityDeposit,
                    AddedBy = lease.AddedBy,
                    UpdatedAt = lease.UpdatedAt
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

        public async Task<bool> DeleteLeaseAsync(long id)
        {
            if (id <= 0) return false;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"DELETE FROM property_leases WHERE id = @Id";

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
