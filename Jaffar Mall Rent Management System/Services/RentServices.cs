using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class RentServices
    {
        private readonly RentRepository _rentRepository;
        private readonly LeasesRepository _leasesRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly PropertyRepository _propertyRepository;

        public RentServices(
            RentRepository rentRepository,
            LeasesRepository leasesRepository,
            TenantRepository tenantRepository,
            PropertyRepository propertyRepository)
        {
            _rentRepository = rentRepository;
            _leasesRepository = leasesRepository;
            _tenantRepository = tenantRepository;
            _propertyRepository = propertyRepository;
        }

        public async Task<BackendResponse<bool>> AddRentAsync(RentPayment payment)
        {
            try
            {
                // Ensure table exists on first run (conceptually)
                await _rentRepository.CreateRentPaymentsTableAsync();

                var lease = await _leasesRepository.GetLeaseByIdAsync(payment.LeaseId);
                if (lease == null || lease.Status != LeaseStatus.Active)
                {
                    return BackendResponse<bool>.Failure("Invalid or inactive lease.", 404);
                }

                if (payment.Amount <= 0)
                {
                    return BackendResponse<bool>.Failure("Rent amount must be greater than zero.", 400);
                }

                bool success = await _rentRepository.AddRentPaymentAsync(payment);
                if (success)
                {
                    return BackendResponse<bool>.Success(true, "Rent payment added successfully.");
                }

                return BackendResponse<bool>.Failure("Failed to add rent payment.", 500);
            }
            catch (Exception ex)
            {
                // Consider logging 'ex' properly
                return BackendResponse<bool>.Failure($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<BackendResponse<IEnumerable<RentStatusViewModel>>> GetRentStatusSummaryAsync()
        {
            try
            {
                // 1. Get all active leases
                var allLeases = await _leasesRepository.GetAllLeasesAsync();
                var activeLeases = allLeases.Where(l => l.Status == LeaseStatus.Active).ToList();
                
                var rentStatusList = new List<RentStatusViewModel>();

                foreach (var lease in activeLeases)
                {
                    // 2. Get details
                    var tenant = await _tenantRepository.GetTenantByIdAsync(lease.TenantId);
                    var property = await _propertyRepository.GetPropertyByIdAsync(lease.PropertyId);
                    
                    if (tenant == null || property == null) continue;

                    // 3. Calculate Expected Rent
                    // Logic: Find how many months have passed since lease creation (or some start date).
                    // For now, assuming lease created date is start date.
                    
                    var startDate = lease.CreatedAt;
                    var today = DateTime.Now;
                    
                    // Simple month difference calculation
                    int monthsPassed = ((today.Year - startDate.Year) * 12) + today.Month - startDate.Month;
                    
                    if (today.Day < startDate.Day) monthsPassed--;
                    
                    if (monthsPassed < 0) monthsPassed = 0;
                    
                    // Add 1 because rent is usually due at start of month
                    // (Adjust logic based on user Requirement: "User lease period is 3 months. It means user have to pay rent after 3 months" -> slightly ambiguous)
                    // Interpreting "It means that the user would have to pay rent after 3 months" as a duration? 
                    // Or implies payment schedule?
                    // User Request: "Property A Assigned to User B and lease period is 3 months. It means user would have to pay rent after 3 months."
                    // This creates ambiguity. Does it mean rent is collected quarterly?
                    // Or lease is ONLY for 3 months?
                    // I will assume standard monthly rent for now, but accumulated over time.
                    // If lease says "3 Months" (duration), and months passed > 3, it might be expired.
                    
                    // Calculating total expected rent till date based on MONTHS passed * Monthly Rent
                    // Assuming payment is due every month.
                    int dueMonths = monthsPassed + 1; // Current month is due
                    
                    // If lease is old, limit due months to lease duration?
                    if (lease.Months > 0 && dueMonths > lease.Months) 
                    {
                        dueMonths = lease.Months;
                    }

                    decimal totalExpected = dueMonths * lease.RentAmount;

                    // 4. Get Total Paid
                    decimal totalPaid = await _rentRepository.GetTotalPaidByLeaseIdAsync(lease.Id);

                    // 5. Balance
                    decimal balance = totalExpected - totalPaid;

                    string status = "Paid";
                    if (balance > 0) status = "Pending";
                    if (balance < 0) status = "Overpaid"; // Optional

                    rentStatusList.Add(new RentStatusViewModel
                    {
                        LeaseId = lease.Id,
                        TenantName = tenant.Name,
                        PropertyName = property.Name,
                        PropertyNumber = property.PropertyNumber,
                        FloorNumber = property.FloorNumber,
                        MonthlyRent = lease.RentAmount,
                        LeaseDurationMonths = lease.Months,
                        LeaseStartDate = startDate,
                        TotalRentExpected = totalExpected,
                        TotalAmountPaid = totalPaid,
                        Balance = balance,
                        Status = status
                    });
                }

                return BackendResponse<IEnumerable<RentStatusViewModel>>.Success(rentStatusList, "Rent status retrieved successfully.");
            }
            catch (Exception ex)
            {
                 return BackendResponse<IEnumerable<RentStatusViewModel>>.Failure($"Error: {ex.Message}", 500);
            }
        }
        public async Task<BackendResponse<IEnumerable<RentPayment>>> GetPaymentsByLeaseIdAsync(long leaseId)
        {
             var payments = await _rentRepository.GetPaymentsByLeaseIdAsync(leaseId);
             return BackendResponse<IEnumerable<RentPayment>>.Success(payments);
        }

        public async Task<BackendResponse<RentPayment>> GetPaymentByIdAsync(long id)
        {
            var payment = await _rentRepository.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return BackendResponse<RentPayment>.Failure("Payment not found.", 404);
            }
            return BackendResponse<RentPayment>.Success(payment);
        }
    }
}
