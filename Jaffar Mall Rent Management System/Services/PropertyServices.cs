using Jaffar_Mall_Rent_Management_System.Backend_Logics;
using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
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
            if (property is null)
                return new BackendResponse<bool>
                {
                    Message = "Property details cannot be empty.",
                    Data = false,
                    Code = 400
                };

            try
            {
                var isAdded = await _propertyRepository.AddPropertyAsync(property);

                return new BackendResponse<bool>
                {
                    Message = isAdded ? "Property added successfully." : "Unable to add property.",
                    Data = isAdded,
                    Code = isAdded ? 200 : 401
                };
            }
            catch (Exception ex)
            {
                // Ideally use ILogger here
                Console.WriteLine($"Error adding property: {ex.Message}");

                return new BackendResponse<bool>
                {
                    Message = "An error occurred while adding property.",
                    Data = false,
                    Code = 500
                };
            }
        }

        public async Task<BackendResponse<bool>> UpdatePropertyAsync(Property property)
        {
            if (property is null)
                return new BackendResponse<bool>
                {
                    Message = "Property details cannot be empty.",
                    Data = false,
                    Code = 400
                };

            try
            {
                var isUpdated = await _propertyRepository.UpdatePropertyAsync(property);

                return new BackendResponse<bool>
                {
                    Message = isUpdated ? "Property updated successfully." : "Unable to update property.",
                    Data = isUpdated,
                    Code = isUpdated ? 200 : 401
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating property: {ex.Message}");
                return new BackendResponse<bool>
                {
                    Message = "An error occurred while updating property.",
                    Data = false,
                    Code = 500
                };
            }
        }

        public async Task<BackendResponse<bool>> DeletePropertyAsync(long id)
        {
            try
            {
                var isDeleted = await _propertyRepository.DeletePropertyAsync(id);

                return new BackendResponse<bool>
                {
                    Message = isDeleted ? "Property deleted successfully." : "Unable to delete property.",
                    Data = isDeleted,
                    Code = isDeleted ? 200 : 404
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting property: {ex.Message}");
                return new BackendResponse<bool>
                {
                    Message = "An error occurred while deleting property.",
                    Data = false,
                    Code = 500
                };
            }
        }

        public async Task<Property?> GetPropertyByIdAsync(long id)
        {
            return await _propertyRepository.GetPropertyByIdAsync(id);
        }

        public async Task<PaginatedViewModel<Property>> GetAllPropertiesAsync(int page, int pageSize, string? searchTerm = null)
        {
            // Calculate skip
            var skip = (page - 1) * pageSize;

            // Run tasks in parallel for efficiency
            var countTask = _propertyRepository.GetTotalPropertiesCountAsync(searchTerm);
            var itemsTask = _propertyRepository.GetAllPropertiesAsync(skip, pageSize, searchTerm);

            await Task.WhenAll(countTask, itemsTask);

            var totalCount = await countTask;
            var items = await itemsTask;

            return new PaginatedViewModel<Property>(items, totalCount, page, pageSize);
        }

        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _propertyRepository.GetAllPropertiesAsync();
        }
    }
}
