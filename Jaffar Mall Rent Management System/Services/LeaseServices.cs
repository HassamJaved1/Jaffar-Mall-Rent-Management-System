using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class LeaseServices
    {
        private readonly PropertyRepository _propertyRepository;

        private readonly TenantRepository _tenantRepository;

        public LeaseServices(PropertyRepository propertyRepository, TenantRepository tenantRepository) 
        {
           _propertyRepository = propertyRepository;
            _tenantRepository = tenantRepository ;


        }

        public async Task<BackendResponse<LeasesDropdown>> PopulateDropdowns()
        {
            var response = new BackendResponse<LeasesDropdown>();
            try
            {
                var properties = await _propertyRepository.GetAllPropertiesAsync();
                var tenants = await _tenantRepository.GetAllTenantsAsync();

                //Task.WhenAll(properties, tenants).Wait();

                //if(properties.Result is not null && tenants.Result is not null)

                //    return BackendResponse<LeasesDropdown>.Success(new LeasesDropdown
                //    {
                //        Properties = properties.Result.ToList(),
                //        Tenants = tenants.Result.ToList()
                //    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error populating dropdowns: {ex.Message}");
            }

            return BackendResponse<LeasesDropdown>.Failure("Failed to retrieve dropdown data.", 500);
        }
    }
}
