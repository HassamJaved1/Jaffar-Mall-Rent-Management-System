using Jaffar_Mall_Rent_Management_System.Backend_Logics;
using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class PropertyServices
    {
        public readonly PropertyRepository _propertyRepository;

        public PropertyServices(PropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        public async Task<BackendResponse<bool>> AddPropertyAsync(Property property)
        {
            var response = new BackendResponse<bool>();

            if (property is null)
                return new BackendResponse<bool>
                {
                    Message = "Username and password cannot be empty.",
                    Data = false,
                    Code = 400
                };

            try
            {
                var isAdded = await _propertyRepository.AddPropertyAsync(property);

                return new BackendResponse<bool>
                {
                    Message = isAdded ? "Property added successful." : "Unable to add property.",
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

        public async Task<Models.ViewModels.PaginatedViewModel<Property>> GetAllPropertiesAsync(int page, int pageSize, string? searchTerm = null)
        {
            // Calculate skip
            var skip = (page - 1) * pageSize;

            // Run tasks in parallel for efficiency
            var countTask = _propertyRepository.GetTotalPropertiesCountAsync(searchTerm);
            var itemsTask = _propertyRepository.GetAllPropertiesAsync(skip, pageSize, searchTerm);

            await Task.WhenAll(countTask, itemsTask);

            var totalCount = await countTask;
            var items = await itemsTask;

            return new Models.ViewModels.PaginatedViewModel<Property>(items, totalCount, page, pageSize);
        }
    }
}
