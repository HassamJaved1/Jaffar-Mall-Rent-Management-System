using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Repositories;
using System.Linq;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class DashboardServices
    {
        private readonly PropertyRepository _propertyRepository;
        private readonly LeasesRepository _leasesRepository;
        private readonly MaintenanceRepository _maintenanceRepository;
        private readonly RentRepository _rentRepository;
        private readonly TenantRepository _tenantRepository;

        public DashboardServices(
            PropertyRepository propertyRepository, 
            LeasesRepository leasesRepository, 
            MaintenanceRepository maintenanceRepository, 
            RentRepository rentRepository,
            TenantRepository tenantRepository)
        {
            _propertyRepository = propertyRepository;
            _leasesRepository = leasesRepository;
            _maintenanceRepository = maintenanceRepository;
            _rentRepository = rentRepository;
            _tenantRepository = tenantRepository;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel();
            var now = DateTime.Now;

            try
            {
                var allLeases = (await _leasesRepository.GetAllLeasesAsync()).ToList();
                var activeLeases = allLeases.Where(l => l.Status == LeaseStatus.Active).ToList();

                // 1. Properties & Occupancy
                var allProperties = (await _propertyRepository.GetAllPropertiesAsync()).ToList();
                var totalProperties = allProperties.Count;
                var occupiedPropertiesCount = activeLeases.Select(l => l.PropertyId).Distinct().Count();
                model.OccupancyRate = totalProperties > 0 ? (double)occupiedPropertiesCount / totalProperties * 100 : 0;

                // 2. Revenue & Refined Pending Logic
                var allPayments = (await _rentRepository.GetAllPaymentsAsync()).ToList();
                model.TotalRevenue = allPayments.Sum(p => p.Amount);
                
                model.PendingRentAmount = 0;
                model.PendingSecurityAmount = 0;

                foreach (var lease in activeLeases)
                {
                    // A. Calculate Pending Security
                    decimal securityOwed = Math.Max(0, (lease.SecurityDeposit ?? 0) - lease.PaidSecurityDeposit);
                    model.PendingSecurityAmount += securityOwed;

                    // B. Calculate Overdue Rent
                    // Number of full rent periods that have passed since the start date
                    var startDate = lease.StartDate ?? lease.CreatedAt;
                    var totalDaysActive = (now - startDate).TotalDays;
                    int periodsPassed = (int)Math.Floor(totalDaysActive / Math.Max(1, lease.RentDueDays));
                    
                    // Total amount that SHOULD have been paid by now for rent
                    decimal totalExpectedRentSoFar = periodsPassed * lease.RentAmount;
                    
                    // Total rent actually paid for this lease
                    decimal totalRentPaid = allPayments.Where(p => p.LeaseId == lease.Id && p.PaymentType == "Rent").Sum(p => p.Amount);
                    
                    decimal rentOverdue = Math.Max(0, totalExpectedRentSoFar - totalRentPaid);
                    model.PendingRentAmount += rentOverdue;
                }
                
                var currentMonthPayments = allPayments.Where(p => p.PaymentDate.Month == now.Month && p.PaymentDate.Year == now.Year).Sum(p => p.Amount);
                // We'll keep the breakdown logic but use the calculated refined values
                
                // 3. Maintenance
                var allMaintenance = (await _maintenanceRepository.GetAllAsync()).ToList();
                var activeMaintenance = allMaintenance.Where(m => (MaintenanceStatus)m.Status != MaintenanceStatus.Resolved && (MaintenanceStatus)m.Status != MaintenanceStatus.Closed).ToList();
                model.ActiveMaintenanceCount = activeMaintenance.Count;

                // 4. Rent Status Breakdown (Updated for Overdue focus)
                decimal totalPotentiallyOwed = activeLeases.Sum(l => l.RentAmount);
                decimal totalSecurityExpected = activeLeases.Sum(l => l.SecurityDeposit).GetValueOrDefault();
                model.RentStatus = new List<RentStatusItem>
                {
                    new RentStatusItem { Label = "Collected (MTD)", Amount = currentMonthPayments, Percentage = totalPotentiallyOwed > 0 ? (double)(currentMonthPayments / totalPotentiallyOwed * 100) : 0, Color = "bg-green-500" },
                    new RentStatusItem { Label = "Overdue Rent", Amount = model.PendingRentAmount, Percentage = totalPotentiallyOwed > 0 ? (double)(model.PendingRentAmount / totalPotentiallyOwed * 100) : 0, Color = "bg-red-500" },
                    new RentStatusItem { Label = "Pending Security", Amount = model.PendingSecurityAmount, Percentage = totalSecurityExpected > 0 ? (double)(model.PendingSecurityAmount / totalSecurityExpected * 100) : 0, Color = "bg-orange-500" }
                };

                // 5. Cash Flow Graph (Last 6 Months)
                model.MonthlyCashFlow = new List<MonthlyCashFlowItem>();
                for (int i = 5; i >= 0; i--)
                {
                    var monthDate = now.AddMonths(-i);
                    var monthPayments = allPayments.Where(p => p.PaymentDate.Month == monthDate.Month && p.PaymentDate.Year == monthDate.Year).Sum(p => p.Amount);
                    var monthExpenses = allMaintenance.Where(m => m.ResolvedAt.HasValue && m.ResolvedAt.Value.Month == monthDate.Month && m.ResolvedAt.Value.Year == monthDate.Year).Sum(m => m.RepairCost);

                    model.MonthlyCashFlow.Add(new MonthlyCashFlowItem
                    {
                        Month = monthDate.ToString("MMM"),
                        Income = monthPayments,
                        Expense = monthExpenses
                    });
                }

                var maxVal = model.MonthlyCashFlow.Any() ? model.MonthlyCashFlow.Max(x => Math.Max(x.Income, x.Expense)) : 0;
                if (maxVal == 0) maxVal = 1;

                foreach (var item in model.MonthlyCashFlow)
                {
                    item.IncomePercentage = (double)(item.Income / maxVal * 100);
                    item.ExpensePercentage = (double)(item.Expense / maxVal * 100);
                }

                // 6. Top Properties
                var tenants = (await _tenantRepository.GetAllTenantsAsync()).ToList();
                model.TopProperties = activeLeases.OrderByDescending(l => l.RentAmount)
                                                  .Take(5)
                                                  .Select(l => new LeaseListItemViewModel {
                                                      Lease = l,
                                                      PropertyName = allProperties.FirstOrDefault(p => p.Id == l.PropertyId)?.Name ?? "Unknown",
                                                      PropertyNumber = allProperties.FirstOrDefault(p => p.Id == l.PropertyId)?.PropertyNumber ?? "N/A",
                                                      TenantName = tenants.FirstOrDefault(t => t.Id == l.TenantId)?.Name ?? "Unknown"
                                                  }).ToList();

                // 7. Alerts
                model.Alerts = new List<DashboardAlert>();
                foreach (var m in activeMaintenance.OrderByDescending(x => x.Priority).Take(3))
                {
                    model.Alerts.Add(new DashboardAlert {
                        Title = m.Title,
                        Description = $"Unit {allProperties.FirstOrDefault(p => p.Id == m.PropertyId)?.PropertyNumber ?? m.PropertyId.ToString()} needs attention.",
                        Icon = m.Priority >= (byte)MaintenancePriority.High ? "warning" : "plumbing",
                        Color = m.Priority >= (byte)MaintenancePriority.High ? "red" : "orange",
                        TimeAgo = "Reported",
                        ActionType = "ViewTicket",
                        ReferenceId = m.Id
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DashboardServices: {ex.Message}");
            }

            return model;
        }
    }
}
