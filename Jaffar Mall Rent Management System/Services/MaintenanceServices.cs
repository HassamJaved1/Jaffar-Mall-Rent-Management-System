using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class MaintenanceServices
    {
        private readonly MaintenanceRepository _maintenanceRepository;
        private readonly PropertyRepository _propertyRepository;
        private readonly TenantRepository _tenantRepository;

        public MaintenanceServices(
            MaintenanceRepository maintenanceRepository, 
            PropertyRepository propertyRepository,
            TenantRepository tenantRepository)
        {
            _maintenanceRepository = maintenanceRepository;
            _propertyRepository = propertyRepository;
            _tenantRepository = tenantRepository;
        }

        public async Task<BackendResponse<long>> AddMaintenanceAsync(Maintenance maintenance)
        {
            try
            {
                // Basic validation
                if (maintenance.PropertyId <= 0)
                {
                    return new BackendResponse<long> { Data = 0, Message = "A valid property must be selected.", Code = 400 };
                }

                // Make sure date values are set
                if (maintenance.CreatedAt == default) maintenance.CreatedAt = DateTime.UtcNow;
                if (maintenance.UpdatedAt == default) maintenance.UpdatedAt = DateTime.UtcNow;

                long id = await _maintenanceRepository.AddMaintenanceAsync(maintenance);
                if (id > 0)
                {
                    return new BackendResponse<long> { Data = id, Message = "Maintenance record added successfully.", Code = 200 };
                }

                return new BackendResponse<long> { Data = 0, Message = "Failed to add maintenance record.", Code = 500 };
            }
            catch (Exception ex)
            {
                return new BackendResponse<long> { Data = 0, Message = $"An error occurred: {ex.Message}", Code = 500 };
            }
        }

        public async Task<BackendResponse<IEnumerable<Maintenance>>> GetAllMaintenanceAsync()
        {
            try
            {
                var list = await _maintenanceRepository.GetAllAsync();
                return new BackendResponse<IEnumerable<Maintenance>> { Data = list, Message = "Retrieved successfully.", Code = 200 };
            }
            catch (Exception ex)
            {
                return new BackendResponse<IEnumerable<Maintenance>> { Data = null!, Message = $"Failed to retrieve records: {ex.Message}", Code = 500 };
            }
        }

        public async Task<BackendResponse<Maintenance?>> GetMaintenanceByIdAsync(long id)
        {
            try
            {
                var record = await _maintenanceRepository.GetByIdAsync(id);
                if (record == null)
                {
                    return new BackendResponse<Maintenance?> { Data = null, Message = "Maintenance record not found.", Code = 404 };
                }
                return new BackendResponse<Maintenance?> { Data = record, Message = "Found successfully.", Code = 200 };
            }
            catch (Exception ex)
            {
                return new BackendResponse<Maintenance?> { Data = null, Message = $"Error: {ex.Message}", Code = 500 };
            }
        }

        public async Task<BackendResponse<bool>> UpdateMaintenanceAsync(Maintenance maintenance)
        {
            try
            {
                bool result = await _maintenanceRepository.UpdateAsync(maintenance);
                if (result)
                {
                    return new BackendResponse<bool> { Data = true, Message = "Maintenance record updated successfully.", Code = 200 };
                }
                return new BackendResponse<bool> { Data = false, Message = "Failed to update maintenance record.", Code = 500 };
            }
            catch (Exception ex)
            {
                return new BackendResponse<bool> { Data = false, Message = $"Error: {ex.Message}", Code = 500 };
            }
        }
    }
}
