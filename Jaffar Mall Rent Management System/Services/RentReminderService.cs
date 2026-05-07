using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    /// <summary>
    /// Background service that runs once per day and sends automated rent reminders:
    /// - 7-day advance notice  → sends to Tenant + Manager
    /// - On due date (no payment) → sends urgent notice to Tenant + Manager
    /// </summary>
    public class RentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RentReminderService> _logger;

        // How often to check (every 24 hours)
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public RentReminderService(IServiceScopeFactory scopeFactory, ILogger<RentReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[RentReminder] Service started. Will check every 24 hours.");

            // Run once immediately on startup, then every 24h
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RentReminder] Unexpected error during processing.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessRemindersAsync()
        {
            _logger.LogInformation("[RentReminder] Running daily check at {Time}", DateTime.Now);

            using var scope = _scopeFactory.CreateScope();
            var leasesRepo   = scope.ServiceProvider.GetRequiredService<LeasesRepository>();
            var tenantRepo   = scope.ServiceProvider.GetRequiredService<TenantRepository>();
            var propertyRepo = scope.ServiceProvider.GetRequiredService<PropertyRepository>();
            var rentRepo     = scope.ServiceProvider.GetRequiredService<RentRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var config       = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            string managerEmail = config["EmailSettings:ManagerEmail"] ?? config["EmailSettings:SmtpUser"] ?? "";

            var today = DateTime.Today;
            var allLeases = await leasesRepo.GetAllLeasesAsync();
            var activeLeases = allLeases.Where(l => l.Status == LeaseStatus.Active).ToList();

            _logger.LogInformation("[RentReminder] Found {Count} active leases to check.", activeLeases.Count);

            foreach (var lease in activeLeases)
            {
                try
                {
                    var tenant   = await tenantRepo.GetTenantByIdAsync(lease.TenantId);
                    var property = await propertyRepo.GetPropertyByIdAsync(lease.PropertyId);

                    if (tenant == null || property == null || string.IsNullOrEmpty(tenant.Email))
                        continue;

                    var nextDueDate = CalculateNextDueDate(lease, today);
                    if (nextDueDate == null) continue;

                    var dueDate = nextDueDate.Value;

                    // Calculate total paid for this period
                    decimal totalPaid = await rentRepo.GetTotalPaidByLeaseIdAsync(lease.Id);

                    // How many full payment periods have elapsed?
                    var startDate  = lease.StartDate ?? lease.CreatedAt;
                    int rentDueMonths = lease.RentDueMonths > 0 ? lease.RentDueMonths : 1;
                    decimal intervalRent = lease.RentAmount * rentDueMonths;

                    int intervalsPaid = (int)(totalPaid / intervalRent);
                    var earliestUnpaidDate = startDate.AddMonths(rentDueMonths * (intervalsPaid + 1));

                    if (lease.EndDate.HasValue && earliestUnpaidDate > lease.EndDate.Value) 
                        continue; // Fully paid up to lease end

                    // Use .Date to ignore time components for logic comparison
                    DateTime todayDate = today.Date;
                    DateTime unpaidDate = earliestUnpaidDate.Date;
                    int daysUntilDue = (unpaidDate - todayDate).Days;

                    // ── Period Opening Reminder (First 5 days of collection period) ────────
                    var periodStartDate = unpaidDate.AddMonths(-rentDueMonths).Date;
                    int daysSincePeriodStarted = (todayDate - periodStartDate).Days;

                    // 1. Initial Period Reminder (Window: Day 0 to Day 4 of the period)
                    if (daysSincePeriodStarted >= 0 && daysSincePeriodStarted < 5)
                    {
                        _logger.LogInformation("[RentReminder] Sending initial period reminder (Day {Day}) → {Tenant} ({Property})", daysSincePeriodStarted, tenant.Name, property.Name);
                        await emailService.SendPendingRentReminderEmailAsync(
                            tenant.Email, tenant.Name, property.Name,
                            intervalRent, unpaidDate, managerEmail);
                    }
                    // 2. 7-day advance reminder
                    else if (daysUntilDue == 7)
                    {
                        _logger.LogInformation("[RentReminder] Sending 7-day reminder → {Tenant} ({Property})", tenant.Name, property.Name);
                        await emailService.SendRentReminderEmailAsync(
                            tenant.Email, tenant.Name, property.Name,
                            intervalRent, unpaidDate, managerEmail);
                    }
                    // 3. Due today not paid (Becomes Pending/Overdue)
                    else if (daysUntilDue == 0)
                    {
                        _logger.LogInformation("[RentReminder] Sending overdue notice → {Tenant} ({Property})", tenant.Name, property.Name);
                        await emailService.SendRentOverdueEmailAsync(
                            tenant.Email, tenant.Name, property.Name,
                            lease.RentAmount, unpaidDate, managerEmail);
                    }
                    // 4. Overdue / Persistent Reminders
                    // Send immediately when the rent first becomes overdue (-1 day),
                    // then continue every 3 days as a reminder.
                    else if (daysUntilDue < 0 && (daysUntilDue == -1 || Math.Abs(daysUntilDue) % 3 == 0))
                    {
                        _logger.LogInformation("[RentReminder] Sending ongoing pending/overdue reminder → {Tenant} ({Property})", tenant.Name, property.Name);
                        await emailService.SendRentOverdueEmailAsync(
                            tenant.Email, tenant.Name, property.Name,
                            lease.RentAmount, unpaidDate, managerEmail);
                    }

                    // ── Automated Rent Increment ───────────────────────
                    if (lease.IncrementMonths > 0 && lease.IncrementPercentage > 0)
                    {
                        var baseDate = lease.LastIncrementDate ?? lease.StartDate ?? lease.CreatedAt;
                        var nextIncrementDue = baseDate.AddMonths(lease.IncrementMonths);

                        if (today >= nextIncrementDue)
                        {
                            decimal oldRent = lease.RentAmount;
                            decimal increaseAmount = oldRent * (lease.IncrementPercentage / 100);
                            decimal newRent = oldRent + increaseAmount;

                            _logger.LogInformation("[RentIncrement] Incrementing rent for Lease {Id} ({Tenant} - {Property}). From {Old} to {New} (+{Percent}%)", 
                                lease.Id, tenant.Name, property.Name, oldRent.ToString("N2"), newRent.ToString("N2"), lease.IncrementPercentage);

                            lease.RentAmount = newRent;
                            lease.LastIncrementDate = today; // Or nextIncrementDue
                            lease.UpdatedAt = DateTime.UtcNow;

                            bool success = await leasesRepo.UpdateLeaseAsync(lease);
                            if (success)
                            {
                                // Optional: Notify tenant/manager about rent increase
                                try {
                                    await emailService.SendRentIncreaseNotificationAsync(
                                        tenant.Email, tenant.Name, property.Name, 
                                        oldRent, newRent, lease.IncrementPercentage, today);
                                } catch (Exception ex) {
                                    _logger.LogWarning("[RentIncrement] Failed to send notification email: {Msg}", ex.Message);
                                }
                            }
                            else
                            {
                                _logger.LogError("[RentIncrement] Failed to update Lease {Id} with new rent.", lease.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RentReminder] Error processing lease {LeaseId}", lease.Id);
                }
            }

            _logger.LogInformation("[RentReminder] Daily check complete.");
        }

        /// <summary>
        /// Calculates the next rent due date based on the lease start date and interval.
        /// Returns the next date that falls on or after today.
        /// </summary>
        private static DateTime? CalculateNextDueDate(PropertyLease lease, DateTime today)
        {
            var startDate = lease.StartDate ?? lease.CreatedAt;
            int rentDueMonths = lease.RentDueMonths > 0 ? lease.RentDueMonths : 1;

            var candidate = startDate.AddMonths(rentDueMonths);

            if (startDate > today) return candidate; // Lease hasn't started yet

            // Find the next due date after or on today
            while (candidate < today)
            {
                 var nextCandidate = candidate.AddMonths(rentDueMonths);
                 if (lease.EndDate.HasValue && nextCandidate > lease.EndDate.Value)
                     break;
                 candidate = nextCandidate;
            }

            // Respect lease end date
            if (lease.EndDate.HasValue && candidate > lease.EndDate.Value)
                return null;

            return candidate;
        }
    }
}
