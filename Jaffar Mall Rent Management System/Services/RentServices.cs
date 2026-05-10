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

                var today = DateTime.Today;

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
                    decimal initialRent = lease.RentAmount;
                    if (initialRent > 0 && lease.IncrementMonths > 0 && lease.IncrementPercentage > 0 && lease.LastIncrementDate.HasValue)
                    {
                        int monthsSinceStart = (lease.LastIncrementDate.Value.Year - startDate.Year) * 12 + lease.LastIncrementDate.Value.Month - startDate.Month;
                        if (lease.LastIncrementDate.Value.Day < startDate.Day)
                        {
                            monthsSinceStart--;
                        }
                        int incrementsApplied = monthsSinceStart / lease.IncrementMonths;
                        for (int inc = 0; inc < incrementsApplied; inc++)
                        {
                            initialRent = initialRent / (1m + lease.IncrementPercentage / 100m);
                        }
                        initialRent = Math.Round(initialRent, 2);
                    }

                    int expectedIntervals = 0;
                    decimal totalExpected = 0;
                    var candidateDate = startDate;
                    decimal currentExpectedIntervalRent = 0;

                    while (true)
                    {
                        currentExpectedIntervalRent = 0;
                        for (int m = 0; m < rentDueMonths; m++)
                        {
                            int monthIndex = expectedIntervals * rentDueMonths + m;
                            int increments = 0;
                            if (lease.IncrementMonths > 0 && lease.IncrementPercentage > 0)
                            {
                                increments = monthIndex / lease.IncrementMonths;
                            }
                            decimal monthRent = initialRent;
                            for (int inc = 0; inc < increments; inc++)
                            {
                                monthRent += monthRent * (lease.IncrementPercentage / 100m);
                            }
                            currentExpectedIntervalRent += monthRent;
                        }
                        
                        expectedIntervals++;
                        totalExpected += currentExpectedIntervalRent;
                        
                        candidateDate = startDate.AddMonths(expectedIntervals * rentDueMonths);
                        if (candidateDate > today || (lease.EndDate.HasValue && candidateDate > lease.EndDate.Value))
                        {
                             break;
                        }
                    }

                    // 4. Get Total Paid
                    decimal totalPaid = await _rentRepository.GetTotalPaidByLeaseIdAsync(lease.Id);

                    int intervalsPaid = 0;
                    decimal accumulatedExpectedRent = 0;
                    decimal intervalRent = 0;

                    if (initialRent > 0)
                    {
                        while (true)
                        {
                            decimal currentIntervalRent = 0;
                            for (int m = 0; m < rentDueMonths; m++)
                            {
                                int monthIndex = intervalsPaid * rentDueMonths + m;
                                int increments = 0;
                                if (lease.IncrementMonths > 0 && lease.IncrementPercentage > 0)
                                {
                                    increments = monthIndex / lease.IncrementMonths;
                                }
                                decimal monthRent = initialRent;
                                for (int inc = 0; inc < increments; inc++)
                                {
                                    monthRent += monthRent * (lease.IncrementPercentage / 100m);
                                }
                                currentIntervalRent += monthRent;
                            }

                            if (currentIntervalRent == 0) break;

                            if (totalPaid >= accumulatedExpectedRent + currentIntervalRent - 0.01m)
                            {
                                accumulatedExpectedRent += currentIntervalRent;
                                intervalsPaid++;
                            }
                            else
                            {
                                intervalRent = currentIntervalRent;
                                break;
                            }
                        }
                    }

                    // Current Paid Rent
                    decimal previousExpected = totalExpected - currentExpectedIntervalRent;
                    decimal currentPaidRent = Math.Max(0, totalPaid - previousExpected);

                    // 5. Balance
                    decimal balance = totalExpected - totalPaid;

                    var earliestUnpaidDueDate = startDate.AddMonths(rentDueMonths * (intervalsPaid + 1));

                    // Calculation for Next Collection Deadline
                    var nextRentDueDate = startDate.AddMonths(rentDueMonths);
                    while (nextRentDueDate < today)
                    {
                        var nextStep = nextRentDueDate.AddMonths(rentDueMonths);
                        if (lease.EndDate.HasValue && nextStep > lease.EndDate.Value) break;
                        nextRentDueDate = nextStep;
                    }

                    string status = "Paid";
                    if (balance < 0)
                    {
                        status = "Overpaid"; // Optional
                    }
                    else if (balance > 0)
                    {
                        status = earliestUnpaidDueDate.Date < today ? "Overdue" : "Pending";
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
                        IntervalRent = currentExpectedIntervalRent,
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

        public async Task<BackendResponse<IEnumerable<RentPayment>>> GetAllPaymentsAsync()
        {
            try
            {
                var payments = await _rentRepository.GetAllPaymentsAsync();
                return BackendResponse<IEnumerable<RentPayment>>.Success(payments, "Retrieved successfully.");
            }
            catch (Exception ex)
            {
                return BackendResponse<IEnumerable<RentPayment>>.Failure($"Failed to retrieve payments: {ex.Message}", 500);
            }
        }
    }
}
