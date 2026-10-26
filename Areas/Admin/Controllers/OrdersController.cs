using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff,NhanVien")]
    public class OrdersController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly OrderManagementService _orderManagement;

        public OrdersController(CoffeeShopDbContext context, OrderManagementService orderManagement)
        {
            _context = context;
            _orderManagement = orderManagement;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var ordersQuery = _context.Orders.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(status))
            {
                ordersQuery = ordersQuery.Where(order => order.TrangThai == status);
            }

            var viewModel = new OrderListViewModel
            {
                SelectedStatus = status,
                Orders = await ordersQuery
                    .OrderByDescending(order => order.NgayTao)
                    .Select(order => new OrderListItemViewModel
                    {
                        MaHD = order.MaHD,
                        HoTenNguoiNhan = order.HoTenNguoiNhan,
                        NgayTao = order.NgayTao,
                        TongTien = order.TongTien,
                        PhuongThucThanhToan = order.PhuongThucThanhToan,
                        TrangThai = order.TrangThai,
                        LyDoHuy = order.LyDoHuy
                    })
                    .ToListAsync()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(item => item.MaHD == id)
                .Select(item => new OrderDetailViewModel
                {
                    MaHD = item.MaHD,
                    NgayTao = item.NgayTao,
                    TrangThai = item.TrangThai,
                    PhuongThucThanhToan = item.PhuongThucThanhToan,
                    NguoiXuLy = item.NguoiXuLy,
                    NgayCapNhat = item.NgayCapNhat,
                    LyDoHuy = item.LyDoHuy,
                    HoTenNguoiNhan = item.HoTenNguoiNhan,
                    SdtNguoiNhan = item.SdtNguoiNhan,
                    EmailKhachHang = item.Customer != null ? item.Customer.Email : null,
                    DiaChiGiao = item.DiaChiGiao,
                    GhiChu = item.GhiChu,
                    TamTinh = item.TamTinh,
                    TienGiamGia = item.TienGiamGia,
                    PhiGiao = item.PhiGiao,
                    TongTien = item.TongTien,
                    Details = item.OrderDetails.Select(detail => new OrderDetailLineViewModel
                    {
                        TenSP = detail.Product!.TenSP,
                        KichCo = detail.Variant!.KichCo,
                        SoLuong = detail.SoLuong,
                        GiaBan = detail.GiaBan,
                        ThanhTien = detail.ThanhTien,
                        Toppings = detail.Toppings
                            .Select(topping => topping.ToppingProduct!.TenSP)
                            .ToList()
                    }).ToList()
                })
                .SingleOrDefaultAsync();

            return order == null ? NotFound() : View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            var result = await _orderManagement.ConfirmAsync(orderId, User.Identity?.Name);
            SetOperationMessage(result, orderId, "xác nhận");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceOrder(int orderId)
        {
            var result = await _orderManagement.AdvanceAsync(orderId, User.Identity?.Name);
            SetOperationMessage(result, orderId, "cập nhật trạng thái");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(CancelOrderRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.CancelReason) || request.CancelReason.Trim().Length > 500)
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy đơn (tối đa 500 ký tự).";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Thông tin yêu cầu hủy đơn không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _orderManagement.CancelAsync(request.OrderId, request.CancelReason, User.Identity?.Name);
            SetOperationMessage(result, request.OrderId, "hủy");
            return RedirectToAction(nameof(Index));
        }

        private void SetOperationMessage(OrderOperationResult result, int orderId, string operation)
        {
            if (result == OrderOperationResult.Success)
            {
                TempData["Success"] = $"Đã {operation} đơn hàng #{orderId}.";
            }
            else if (result == OrderOperationResult.NotFound)
            {
                TempData["Error"] = $"Không tìm thấy đơn hàng #{orderId}.";
            }
            else if (result == OrderOperationResult.InvalidRequest)
            {
                TempData["Error"] = "Lý do hủy không hợp lệ.";
            }
            else
            {
                TempData["Error"] = $"Không thể {operation} đơn hàng #{orderId} do trạng thái hiện tại không hợp lệ.";
            }
        }
    }
}
