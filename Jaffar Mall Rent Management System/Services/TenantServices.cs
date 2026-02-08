using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
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
                Console.WriteLine($"Error adding tenant {ex.Message}");

                return new BackendResponse<bool>
                {
                    Message = "An error occurred while adding tenant.",
                    Data = false,
                    Code = 500
                };
            }
        }

        public async Task<PaginatedViewModel<Tenant>> GetAllTenantsAsync(int page, int pageSize, string? searchTerm = null)
        {
            // Calculate skip
            var skip = (page - 1) * pageSize;

            // Run tasks in parallel
            var countTask = _tenantRepository.GetTotalTenantsCountAsync(searchTerm);
            var itemsTask = _tenantRepository.GetAllTenantsAsync(skip, pageSize, searchTerm);

            await Task.WhenAll(countTask, itemsTask);

            var totalCount = await countTask;
            var items = await itemsTask;

            return new PaginatedViewModel<Tenant>(items, totalCount, page, pageSize);
        }
    }
}
