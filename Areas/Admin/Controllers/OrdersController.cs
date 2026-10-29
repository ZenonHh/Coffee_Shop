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

        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            const int pageSize = 10;
            var ordersQuery = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Variant)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                ordersQuery = ordersQuery.Where(order => order.TrangThai == status);
            }

            var totalCount = await ordersQuery.CountAsync();
            page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize)));
            var viewModel = new OrderListViewModel
            {
                SelectedStatus = status,
                Pagination = new PaginationViewModel { PageNumber = page, PageSize = pageSize, TotalCount = totalCount },
                Orders = await ordersQuery
                    .OrderByDescending(order => order.MaHD)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(order => new OrderListItemViewModel
                    {
                        MaHD = order.MaHD,
                        HoTenNguoiNhan = order.HoTenNguoiNhan,
                        SdtNguoiNhan = order.SdtNguoiNhan,
                        DiaChiGiao = order.DiaChiGiao,
                        GhiChu = order.GhiChu,
                        NgayTao = order.NgayTao,
                        TongTien = order.TongTien,
                        PhuongThucThanhToan = order.PhuongThucThanhToan,
                        TrangThai = order.TrangThai,
                        LyDoHuy = order.LyDoHuy,
                        TotalItems = order.OrderDetails.Sum(d => d.SoLuong),
                        ItemsPreview = order.OrderDetails.Select(d => $"{d.SoLuong}x {(d.Product != null ? d.Product.TenSP : "Món")} (Size {(d.Variant != null ? d.Variant.KichCo : "Vừa")})").Take(4).ToList()
                    })
                    .ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = $"Không tìm thấy đơn hàng #{orderId}.";
                return RedirectToAction(nameof(Index));
            }

            var isTransfer = order.PhuongThucThanhToan.Contains("Transfer", StringComparison.OrdinalIgnoreCase)
                || order.PhuongThucThanhToan.Contains("Chuyển khoản", StringComparison.OrdinalIgnoreCase);
            if (order.TrangThai != OrderStatuses.WaitingForConfirmation || !isTransfer
                || order.PhuongThucThanhToan.Contains("Đã thanh toán", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Chỉ có thể xác nhận khoản chuyển khoản chưa thanh toán của đơn đang chờ xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            order.PhuongThucThanhToan = "Chuyển khoản (Đã thanh toán)";
            order.NguoiXuLy = User.Identity?.Name ?? "Quản trị viên";
            order.NgayCapNhat = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã xác nhận thanh toán cho đơn hàng #HD{order.MaHD:D4}. Trạng thái đơn vẫn chờ xác nhận.";
            return RedirectToAction(nameof(Index));
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
