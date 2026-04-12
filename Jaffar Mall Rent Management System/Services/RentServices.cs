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
                        var currentStatus = statusSummary.Data?.FirstOrDefault(s => s.LeaseId == lease.Id);
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
                            securityBalance,
                            payment);
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
                    var startDate = lease.StartDate ?? lease.CreatedAt;
                    var endDate = lease.EndDate;
                    int rentDueMonths = lease.RentDueMonths > 0 ? lease.RentDueMonths : 1;
                    decimal intervalRent = lease.RentAmount * rentDueMonths;

                    int expectedIntervals = 1;
                    var candidateDate = startDate.AddMonths(rentDueMonths);
                    while (candidateDate <= DateTime.Now)
                    {
                         if (lease.EndDate.HasValue && candidateDate > lease.EndDate.Value) break;
                         expectedIntervals++;
                         candidateDate = candidateDate.AddMonths(rentDueMonths);
                    }
                    
                    decimal totalExpected = expectedIntervals * intervalRent;

                    // 4. Get Total Paid
                    decimal totalPaid = await _rentRepository.GetTotalPaidByLeaseIdAsync(lease.Id);

                    // Current Paid Rent
                    decimal previousExpected = totalExpected - intervalRent;
                    decimal currentPaidRent = Math.Max(0, totalPaid - previousExpected);

                    // 5. Balance
                    decimal balance = totalExpected - totalPaid;

                    string status = "Paid";
                    if (balance > 0) status = "Pending";
                    if (balance < 0) status = "Overpaid"; // Optional

                    // Calculation for Next Collection Deadline
                    var nextRentDueDate = startDate.AddMonths(rentDueMonths);
                    while (nextRentDueDate < DateTime.Now)
                    {
                        var nextStep = nextRentDueDate.AddMonths(rentDueMonths);
                        if (lease.EndDate.HasValue && nextStep > lease.EndDate.Value) break;
                        nextRentDueDate = nextStep;
                    }

                    rentStatusList.Add(new RentStatusViewModel
                    {
                        LeaseId = lease.Id,
                        PropertyId = property.Id,
                        TenantName = tenant.Name,
                        PropertyName = property.Name,
                        PropertyNumber = property.PropertyNumber,
                        FloorNumber = property.FloorNumber,
                        MonthlyRent = lease.RentAmount,
                        LeaseDurationMonths = lease.Months,
                        SecurityDeposit = lease.SecurityDeposit,
                        RentDueMonths = lease.RentDueMonths,
                        LeaseStartDate = startDate,
                        LeaseEndDate = endDate,
                        NextRentDueDate = nextRentDueDate,
                        IntervalRent = intervalRent,
                        TotalRentExpected = totalExpected,
                        TotalAmountPaid = totalPaid,
                        CurrentPaidRent = currentPaidRent,
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
