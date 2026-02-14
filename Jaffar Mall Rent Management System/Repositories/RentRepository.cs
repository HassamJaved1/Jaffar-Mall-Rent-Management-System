using Dapper;
using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;

namespace Jaffar_Mall_Rent_Management_System.Repositories
{
    public class RentRepository
    {
        private readonly string _connectionString;

        public RentRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Ensure table exists (simple migration mechanism)
        public async Task CreateRentPaymentsTableAsync()
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Using PostgreSQL syntax
                const string sql = @"
                CREATE TABLE IF NOT EXISTS rent_payments (
                    id BIGSERIAL PRIMARY KEY,
                    lease_id BIGINT NOT NULL,
                    amount DECIMAL(18, 2) NOT NULL,
                    payment_date TIMESTAMP NOT NULL DEFAULT NOW(),
                    payment_method VARCHAR(100) NOT NULL,
                    remarks TEXT,
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    CONSTRAINT fk_lease
                        FOREIGN KEY(lease_id) 
                        REFERENCES property_leases(id)
                        ON DELETE CASCADE
                );";

                await connection.ExecuteAsync(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating rent_payments table: {ex.Message}");
            }
        }

        public async Task<bool> AddRentPaymentAsync(RentPayment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                INSERT INTO rent_payments
                    (lease_id, amount, payment_date, payment_method, remarks, created_at)
                VALUES
                    (@LeaseId, @Amount, @PaymentDate, @PaymentMethod, @Remarks, @CreatedAt)
                RETURNING id";

                var parameters = new
                {
                    LeaseId = payment.LeaseId,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate,
                    PaymentMethod = payment.PaymentMethod,
                    Remarks = payment.Remarks,
                    CreatedAt = payment.CreatedAt
                };

                long insertedId = await connection.ExecuteScalarAsync<long>(sql, parameters);

                if (insertedId > 0)
                {
                    payment.Id = insertedId;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding rent payment: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<RentPayment>> GetPaymentsByLeaseIdAsync(long leaseId)
        {
            if (leaseId <= 0) return Array.Empty<RentPayment>();

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    id,
                    lease_id AS ""LeaseId"",
                    amount,
                    payment_date AS ""PaymentDate"",
                    payment_method AS ""PaymentMethod"",
                    remarks,
                    created_at AS ""CreatedAt""
                FROM rent_payments
                WHERE lease_id = @LeaseId
                ORDER BY payment_date DESC";

                return await connection.QueryAsync<RentPayment>(sql, new { LeaseId = leaseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<RentPayment>();
            }
        }

        // Get total paid amount for a specific lease
        public async Task<decimal> GetTotalPaidByLeaseIdAsync(long leaseId)
        {
             if (leaseId <= 0) return 0;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"SELECT COALESCE(SUM(amount), 0) FROM rent_payments WHERE lease_id = @LeaseId";

                return await connection.ExecuteScalarAsync<decimal>(sql, new { LeaseId = leaseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
        // Get a single payment by ID
        public async Task<RentPayment?> GetPaymentByIdAsync(long id)
        {
            if (id <= 0) return null;

            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                SELECT
                    id,
                    lease_id AS ""LeaseId"",
                    amount,
                    payment_date AS ""PaymentDate"",
                    payment_method AS ""PaymentMethod"",
                    remarks,
                    created_at AS ""CreatedAt""
                FROM rent_payments
                WHERE id = @Id";

                return await connection.QuerySingleOrDefaultAsync<RentPayment?>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
