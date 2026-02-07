using AspNetCoreGeneratedDocument;
using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class TenantServices
    {
        private readonly TenantRepository _tenantRepository;

        public TenantServices(TenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<BackendResponse<bool>> AddTenantAsync(Tenant tenant)
        {
            var response = new BackendResponse<bool>();

            if (tenant is null)
                return new BackendResponse<bool>
                {
                    Message = "Tenant details cannot be null.",
                    Data = false,
                    Code = 400
                };

            try
            {
                var isAdded = await _tenantRepository.AddTenantAsync(tenant);

                return new BackendResponse<bool>
                {
                    Message = isAdded ? "Tenant added successful." : "Unable to add Tenant.",
                    Data = isAdded,
                    Code = isAdded ? 200 : 401
                };
            }
            catch (Exception ex)
            {
                // Ideally use ILogger here
                Console.WriteLine($"Error authenticating us {ex.Message}");

                return new BackendResponse<bool>
                {
                    Message = "An error occurred while authenticating.",
                    Data = false,
                    Code = 500
                };
            }
        }

    }
}
