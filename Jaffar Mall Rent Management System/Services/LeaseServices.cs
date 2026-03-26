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
                var propertiesTask = _propertyRepository.GetVacantPropertiesAsync();
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
        
        public async Task<IEnumerable<PropertyLease>> GetAllLeasesAsync() 
        {
            return await _leasesRepository.GetAllLeasesAsync();
        }

        public async Task<IEnumerable<PropertyLease>> GetLeasesByPropertyIdAsync(long propertyId)
        {
            return await _leasesRepository.GetLeasesByPropertyIdAsync(propertyId);
        }

        public async Task<IEnumerable<PropertyLease>> GetLeasesByTenantIdAsync(long tenantId)
        {
            return await _leasesRepository.GetLeasesByTenantIdAsync(tenantId);
        }

        public async Task<PropertyLease?> GetLeaseByIdAsync(long id)
        {
            return await _leasesRepository.GetLeaseByIdAsync(id);
        }

        public async Task<BackendResponse<bool>> UpdateLeaseAsync(PropertyLease lease)
        {
            if (lease == null || lease.Id <= 0) return BackendResponse<bool>.Failure("Invalid lease data.", 400);

            try
            {
                var result = await _leasesRepository.UpdateLeaseAsync(lease);
                return result ? BackendResponse<bool>.Success(true, "Lease updated successfully.") : BackendResponse<bool>.Failure("Failed to update lease.", 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BackendResponse<bool>.Failure("An error occurred.", 500);
            }
        }

        public async Task<BackendResponse<bool>> TerminateLeaseAsync(long id)
        {
            try
            {
                var lease = await _leasesRepository.GetLeaseByIdAsync(id);
                if (lease == null) return BackendResponse<bool>.Failure("Lease not found.", 404);

                lease.Status = LeaseStatus.Terminated;
                var result = await _leasesRepository.UpdateLeaseAsync(lease);
                return result ? BackendResponse<bool>.Success(true, "Lease terminated. Property is now vacant.") : BackendResponse<bool>.Failure("Failed to terminate lease.", 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BackendResponse<bool>.Failure("An error occurred.", 500);
            }
        }

        public async Task<IEnumerable<LeaseListItemViewModel>> GetLeaseManagementListAsync()
        {
            var leases = await _leasesRepository.GetAllLeasesAsync();
            var list = new List<LeaseListItemViewModel>();

            foreach (var lease in leases)
            {
                var tenant = await _tenantRepository.GetTenantByIdAsync(lease.TenantId);
                var property = await _propertyRepository.GetPropertyByIdAsync(lease.PropertyId);

                list.Add(new LeaseListItemViewModel
                {
                    Lease = lease,
                    TenantName = tenant?.Name ?? "Unknown",
                    PropertyName = property?.Name ?? "Unknown",
                    PropertyNumber = property?.PropertyNumber ?? "N/A"
                });
            }

            return list;
        }
    }
}
