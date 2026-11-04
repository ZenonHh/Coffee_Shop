using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff,NhanVien")]
    public class DashboardController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public DashboardController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalCustomerCount = await _context.Customers.CountAsync(),
                PendingReviewCount = await _context.Feedbacks.CountAsync(feedback =>
                    feedback.PhanHoiQuanTri == null || feedback.PhanHoiQuanTri == ""),
                PendingContactCount = await _context.ContactMessages.CountAsync(message => message.TrangThai == "Chưa xử lý"),
                ActiveSurveyCount = await _context.Surveys.CountAsync(survey =>
                    survey.NgayBatDau <= DateTime.Now && survey.NgayKetThuc >= DateTime.Now)
            };

            if (User.IsInRole("Admin"))
            {
                viewModel.TotalRevenue = await _context.Orders
                    .Where(order => order.TrangThai == OrderStatuses.Completed
                        || order.TrangThai == OrderStatuses.LegacyCompleted)
                    .SumAsync(order => order.TongTien);
                viewModel.PendingOrderCount = await _context.Orders.CountAsync(order =>
                    order.TrangThai == OrderStatuses.WaitingForConfirmation);
            }

            return View(viewModel);
        }
    }
}
