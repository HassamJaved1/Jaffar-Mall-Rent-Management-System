using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class LeaseServices
    {
        private readonly PropertyRepository _propertyRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly LeasesRepository _leasesRepository;

        public LeaseServices(PropertyRepository propertyRepository, TenantRepository tenantRepository, LeasesRepository leasesRepository) 
        {
           _propertyRepository = propertyRepository;
            _tenantRepository = tenantRepository ;
            _leasesRepository = leasesRepository;
        }

        public async Task<BackendResponse<LeasesDropdown>> PopulateDropdowns()
        {
            var response = new BackendResponse<LeasesDropdown>();
            try
            {
                var propertiesTask = _propertyRepository.GetAllPropertiesAsync();
                var tenantsTask = _tenantRepository.GetAllTenantsAsync();

                await Task.WhenAll(propertiesTask, tenantsTask);
                
                var properties = await propertiesTask;
                var tenants = await tenantsTask;


                return BackendResponse<LeasesDropdown>.Success(new LeasesDropdown
                {
                    Properties = properties.OrderBy(p => p.Name).ToList(),
                    Tenants = tenants.OrderBy(t => t.Name).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error populating dropdowns: {ex.Message}");
                return BackendResponse<LeasesDropdown>.Failure("Failed to retrieve dropdown data.", 500);
            }
        }

        public async Task<BackendResponse<bool>> AddLeaseAsync(PropertyLease lease)
        {
            if (lease == null)
            {
                return BackendResponse<bool>.Failure("Lease cannot be null.", 400);
            }

            try
            {
                var result = await _leasesRepository.AddLeaseAsync(lease);
                if (result)
                {
                    return BackendResponse<bool>.Success(true, "Lease created successfully.");
                }
                else
                {
                    return BackendResponse<bool>.Failure("Failed to create lease.", 500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating lease: {ex.Message}");
                return BackendResponse<bool>.Failure("An error occurred while creating the lease.", 500);
            }
        }
    }
}
