using System.Data;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Services
{
    public static class OrderStatuses
    {
        public const string WaitingForConfirmation = "Chờ xác nhận";
        public const string Confirmed = "Đã xác nhận";
        public const string Delivering = "Đang giao";
        public const string Completed = "Hoàn thành";
        public const string LegacyCompleted = "Hoàn tất";
        public const string Cancelled = "Đã hủy";

        public static string Canonicalize(string status) =>
            status == LegacyCompleted ? Completed : status;

        public static bool CanConfirm(string status) => status == WaitingForConfirmation;

        public static bool CanCancel(string status) =>
            status == WaitingForConfirmation || status == Confirmed;
    }

    public enum OrderOperationResult
    {
        Success,
        NotFound,
        InvalidState,
        InvalidRequest
    }

    public class OrderManagementService
    {
        private readonly CoffeeShopDbContext _context;

        public OrderManagementService(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<OrderOperationResult> ConfirmAsync(int orderId, string? processedBy, int staffId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var order = await _context.Orders.SingleOrDefaultAsync(o => o.MaHD == orderId);
            if (order == null)
            {
                return OrderOperationResult.NotFound;
            }

            if (!OrderStatuses.CanConfirm(order.TrangThai))
            {
                return OrderOperationResult.InvalidState;
            }

            order.TrangThai = OrderStatuses.Confirmed;
            order.NguoiXuLy = processedBy;
            order.MaNV ??= staffId;
            order.NgayCapNhat = DateTime.Now;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return OrderOperationResult.Success;
        }

        public async Task<OrderOperationResult> AdvanceAsync(int orderId, string? processedBy, int staffId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var order = await _context.Orders.SingleOrDefaultAsync(o => o.MaHD == orderId);
            if (order == null)
            {
                return OrderOperationResult.NotFound;
            }

            var nextStatus = order.TrangThai switch
            {
                OrderStatuses.Confirmed => OrderStatuses.Delivering,
                OrderStatuses.Delivering => OrderStatuses.Completed,
                _ => null
            };
            if (nextStatus == null)
            {
                return OrderOperationResult.InvalidState;
            }

            order.TrangThai = nextStatus;
            order.NguoiXuLy = processedBy;
            order.MaNV ??= staffId;
            order.NgayCapNhat = DateTime.Now;
            if (nextStatus == OrderStatuses.Completed)
            {
                await AwardCompletionPointsAsync(order);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return OrderOperationResult.Success;
        }

        private async Task AwardCompletionPointsAsync(Order order)
        {
            if (!order.MaKH.HasValue)
            {
                return;
            }

            var pointsEarned = (int)(order.TongTien / 10000m);
            if (pointsEarned <= 0
                || await _context.PointHistories.AnyAsync(history =>
                    history.MaHD == order.MaHD && history.DiemThayDoi > 0))
            {
                return;
            }

            var customer = await _context.Customers
                .SingleOrDefaultAsync(item => item.MaKH == order.MaKH.Value);
            if (customer is null)
            {
                return;
            }

            customer.DiemTichLuy += pointsEarned;
            if (customer.DiemTichLuy >= 500) customer.HangTV = "Kim Cương";
            else if (customer.DiemTichLuy >= 200) customer.HangTV = "Vàng";
            else if (customer.DiemTichLuy >= 50) customer.HangTV = "Bạc";

            _context.PointHistories.Add(new PointHistory
            {
                MaKH = customer.MaKH,
                MaHD = order.MaHD,
                DiemThayDoi = pointsEarned,
                LyDo = $"Tích điểm đơn hàng #{order.MaHD}",
                NgayThucHien = DateTime.Now
            });
        }

        public async Task<OrderOperationResult> CancelAsync(int orderId, string cancelReason, string? processedBy)
        {
            if (string.IsNullOrWhiteSpace(cancelReason) || cancelReason.Trim().Length > 500)
            {
                return OrderOperationResult.InvalidRequest;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(detail => detail.Toppings)
                .SingleOrDefaultAsync(o => o.MaHD == orderId);
            if (order == null)
            {
                return OrderOperationResult.NotFound;
            }

            if (!OrderStatuses.CanCancel(order.TrangThai))
            {
                return OrderOperationResult.InvalidState;
            }

            var stockToRestore = new Dictionary<int, int>();
            foreach (var detail in order.OrderDetails)
            {
                stockToRestore[detail.MaSP] = stockToRestore.GetValueOrDefault(detail.MaSP) + detail.SoLuong;
                foreach (var topping in detail.Toppings)
                {
                    stockToRestore[topping.MaSP] = stockToRestore.GetValueOrDefault(topping.MaSP) + detail.SoLuong;
                }
            }

            foreach (var (productId, quantity) in stockToRestore)
            {
                var updated = await _context.Products
                    .Where(product => product.MaSP == productId)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(product => product.SoLuongTon, product => product.SoLuongTon + quantity));
                if (updated != 1)
                {
                    throw new InvalidOperationException($"Không thể hoàn kho sản phẩm #{productId} của đơn hàng #{orderId}.");
                }
            }

            order.TrangThai = OrderStatuses.Cancelled;
            order.LyDoHuy = cancelReason.Trim();
            order.NguoiXuLy = processedBy;
            order.NgayCapNhat = DateTime.Now;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return OrderOperationResult.Success;
        }
    }
}
