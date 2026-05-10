using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class VouchersController : Controller
    {
        private readonly RentServices _rentServices;
        private readonly MaintenanceServices _maintenanceServices;

        public VouchersController(RentServices rentServices, MaintenanceServices maintenanceServices)
        {
            _rentServices = rentServices;
            _maintenanceServices = maintenanceServices;
        }

        public async Task<IActionResult> Index()
        {
            var vouchers = new List<VoucherListItemViewModel>();

            var rentPaymentsResult = await _rentServices.GetAllPaymentsAsync();
            if (rentPaymentsResult.Data != null)
            {
                foreach (var rp in rentPaymentsResult.Data)
                {
                    vouchers.Add(new VoucherListItemViewModel
                    {
                        Type = "Rent",
                        Title = $"{rp.PaymentType} Payment",
                        Amount = rp.Amount,
                        Date = rp.PaymentDate,
                        Status = "Paid",
                        LinkUrl = Url.Action("Voucher", "Rent", new { id = rp.Id }) ?? string.Empty
                    });
                }
            }

            var maintenanceResult = await _maintenanceServices.GetAllMaintenanceAsync();
            if (maintenanceResult.Data != null)
            {
                foreach (var m in maintenanceResult.Data)
                {
                    vouchers.Add(new VoucherListItemViewModel
                    {
                        Type = "Maintenance",
                        Title = m.Title,
                        Amount = m.RepairCost,
                        Date = m.CreatedAt,
                        Status = m.Status == 1 ? "Pending" : "Resolved",
                        LinkUrl = Url.Action("Voucher", "Maintenance", new { id = m.Id }) ?? string.Empty
                    });
                }
            }

            var sortedVouchers = vouchers.OrderByDescending(v => v.Date).ToList();

            return View(sortedVouchers);
        }
    }
}
