using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Repositories;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class LeaseExpiryReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<LeaseExpiryReminderService> _logger;
        private readonly HashSet<long> _notifiedLeaseIds = new();
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

        public LeaseExpiryReminderService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<LeaseExpiryReminderService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await NotifyExpiredLeasesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing lease expiry reminders.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task NotifyExpiredLeasesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var leaseRepository = scope.ServiceProvider.GetRequiredService<LeasesRepository>();
            var tenantRepository = scope.ServiceProvider.GetRequiredService<TenantRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var leases = await leaseRepository.GetAllLeasesAsync();
            var activeExpiredLeases = leases
                .Where(l => l.Status == LeaseStatus.Active)
                .Where(l => DateTime.UtcNow >= l.CreatedAt.AddMonths(l.Months));

            foreach (var lease in activeExpiredLeases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_notifiedLeaseIds.Contains(lease.Id))
                {
                    continue;
                }

                var tenant = await tenantRepository.GetTenantByIdAsync(lease.TenantId);
                if (tenant is null || string.IsNullOrWhiteSpace(tenant.Email))
                {
                    _logger.LogWarning("Skipped lease {LeaseId}: tenant email not available.", lease.Id);
                    continue;
                }

                var expirationDate = lease.CreatedAt.AddMonths(lease.Months);
                var subject = "Rent Collection Period Exceeded";
                var body =
                    $"Dear {tenant.Name},\n\n" +
                    $"Your lease rent collection interval of {lease.Months} month(s) has passed on {expirationDate:yyyy-MM-dd}. " +
                    "Please contact management to proceed with the next payment cycle.\n\n" +
                    "Regards,\nMall Rent Management";

                await emailService.SendEmailAsync(tenant.Email, subject, body);
                _notifiedLeaseIds.Add(lease.Id);

                _logger.LogInformation("Expiry reminder sent for lease {LeaseId} to tenant {TenantId}.", lease.Id, tenant.Id);
            }
        }
    }
}
