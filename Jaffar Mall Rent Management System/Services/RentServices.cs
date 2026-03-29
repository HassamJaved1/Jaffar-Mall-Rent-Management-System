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
            PropertyRepository propertyRepository,
            EmailService emailService)
        {
            _rentRepository = rentRepository;
            _leasesRepository = leasesRepository;
            _tenantRepository = tenantRepository;
            _propertyRepository = propertyRepository;
            _emailService = emailService;
        }
        
        private readonly EmailService _emailService;

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
                    return BackendResponse<bool>.Failure("Amount must be greater than zero.", 400);
                }

                // Handle Security Fee payment
                if (payment.PaymentType == "Security Fee")
                {
                    lease.PaidSecurityDeposit += payment.Amount;
                    await _leasesRepository.UpdateLeaseAsync(lease);
                }

                bool success = await _rentRepository.AddRentPaymentAsync(payment);
                if (success)
                {
                    // Trigger confirmation email
                    var tenant = await _tenantRepository.GetTenantByIdAsync(lease.TenantId);
                    var property = await _propertyRepository.GetPropertyByIdAsync(lease.PropertyId);
                    
                    if (tenant != null && property != null)
                    {
                        // Calculate remaining balance for the notification
                        var statusSummary = await GetRentStatusSummaryAsync();
                        var currentStatus = statusSummary.Data.FirstOrDefault(s => s.LeaseId == lease.Id);
                        decimal balance = currentStatus?.Balance ?? 0;
                        decimal securityBalance = currentStatus?.SecurityBalance ?? 0;
                        
                        // Use relaxation days from the payment record
                        int relaxationDays = payment.RelaxationDays; 

                        await _emailService.SendPaymentConfirmationEmailAsync(
                            tenant.Email ?? "", 
                            tenant.Name, 
                            property.Name, 
                            payment.Amount, 
                            balance, 
                            payment.PaymentType, 
                            relaxationDays,
                            securityBalance);
                    }

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
                var activeLeases = allLeases.Where(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending).ToList();
                
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
                    
                    var startDate = lease.StartDate ?? lease.CreatedAt;
                    var endDate = lease.EndDate;
                    var today = DateTime.Now;
                    
                    // Simple month difference calculation
                    int monthsPassed = ((today.Year - startDate.Year) * 12) + today.Month - startDate.Month;
                    
                    if (today.Day < startDate.Day) monthsPassed--;
                    
                    if (monthsPassed < 0) monthsPassed = 0;
                    
                    // Add 1 because rent is usually due at start of month
                    int dueMonths = monthsPassed + 1; // Current month is due
                    
                    // Ensure dueMonths doesn't exceed total expected months in the lease
                    if (endDate.HasValue) 
                    {
                        int totalLeaseMonths = ((endDate.Value.Year - startDate.Year) * 12) + endDate.Value.Month - startDate.Month;
                        if (endDate.Value.Day >= startDate.Day) totalLeaseMonths++;
                        
                        // We also respect lease.Months if it's there as a fallback
                        int maxMonths = lease.Months > 0 ? lease.Months : totalLeaseMonths;
                        if (dueMonths > maxMonths) 
                        {
                            dueMonths = maxMonths;
                        }
                    } 
                    else if (lease.Months > 0 && dueMonths > lease.Months) 
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
                        SecurityDeposit = lease.SecurityDeposit,
                        RentDueDays = lease.RentDueDays,
                        LeaseStartDate = startDate,
                        LeaseEndDate = endDate,
                        TotalRentExpected = totalExpected,
                        TotalAmountPaid = totalPaid,
                        Balance = balance,
                        PaidSecurityDeposit = lease.PaidSecurityDeposit,
                        SecurityBalance = (lease.SecurityDeposit ?? 0) - lease.PaidSecurityDeposit,
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
