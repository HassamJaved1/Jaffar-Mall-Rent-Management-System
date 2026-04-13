using Dapper;
using Jaffar_Mall_Rent_Management_System.Models;
using Npgsql;

namespace Jaffar_Mall_Rent_Management_System.Repositories
{
    public class DocumentRepository
    {
        private readonly string _connectionString;

        public DocumentRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task CreateTableIfNotExistsAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                CREATE TABLE IF NOT EXISTS documents (
                    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    description TEXT,
                    tenant_id BIGINT,
                    file_path TEXT NOT NULL,
                    uploaded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT fk_tenant FOREIGN KEY(tenant_id) REFERENCES tenants(id) ON DELETE SET NULL
                );
            ";
            await connection.ExecuteAsync(sql);
        }

        public async Task<bool> AddDocumentAsync(Document document)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    INSERT INTO documents (description, tenant_id, file_path, uploaded_at)
                    VALUES (@Description, @TenantId, @FilePath, @UploadedAt)
                    RETURNING id;";

                var id = await connection.ExecuteScalarAsync<long>(sql, new
                {
                    document.Description,
                    document.TenantId,
                    document.FilePath,
                    document.UploadedAt
                });

                if (id > 0)
                {
                    document.Id = id;
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

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT d.id as Id, d.description as Description, d.tenant_id as TenantId, 
                           d.file_path as FilePath, d.uploaded_at as UploadedAt,
                           t.name as TenantName
                    FROM documents d
                    LEFT JOIN tenants t ON d.tenant_id = t.id
                    ORDER BY d.uploaded_at DESC;
                ";

                return await connection.QueryAsync<Document>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<Document>();
            }
        }

        public async Task<Document?> GetDocumentByIdAsync(long id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT d.id as Id, d.description as Description, d.tenant_id as TenantId, 
                           d.file_path as FilePath, d.uploaded_at as UploadedAt
                    FROM documents d
                    WHERE d.id = @Id;
                ";

                return await connection.QuerySingleOrDefaultAsync<Document>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> DeleteDocumentAsync(long id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = "DELETE FROM documents WHERE id = @Id;";
                var rows = await connection.ExecuteAsync(sql, new { Id = id });
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
