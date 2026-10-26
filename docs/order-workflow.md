# Đặc tả nghiệp vụ đơn hàng

## Máy trạng thái đơn hàng

| Trạng thái hiện tại | Sự kiện/Actor | Điều kiện | Trạng thái kế tiếp | Tác động |
|---|---|---|---|---|
| Đặt hàng thành công | Khách hàng đặt hàng | Giỏ hàng, sản phẩm, topping và thanh toán hợp lệ | Chờ xác nhận | Ghi hóa đơn/chi tiết, trừ tồn kho, ghi phương thức thanh toán |
| Chờ xác nhận | Admin xác nhận | Đơn tồn tại và chưa xử lý | Đã xác nhận | Ghi người xử lý và thời điểm cập nhật |
| Đã xác nhận | Admin bắt đầu giao | Đơn đang ở trạng thái Đã xác nhận | Đang giao | Ghi người xử lý và thời điểm cập nhật |
| Đang giao | Admin hoàn tất | Đơn đang ở trạng thái Đang giao | Hoàn thành | Ghi người xử lý và thời điểm cập nhật |
| Chờ xác nhận hoặc Đã xác nhận | Admin hủy, nhập lý do | Lý do sau khi trim không rỗng, tối đa 500 ký tự | Đã hủy | Ghi lý do/người xử lý/thời điểm; hoàn kho món chính và topping trong cùng transaction |
| Hoàn thành hoặc Đã hủy | Mọi sự kiện đổi trạng thái | Trạng thái cuối | Không đổi | Từ chối chuyển trạng thái; không hoàn kho lần hai |

## Luồng dữ liệu (Sequence/Activity)

1. **Khách đặt hàng:** Khách hàng → Razor Checkout → `CheckoutController.PlaceOrder` → kiểm tra giỏ hàng/voucher/tồn kho → transaction SQL Server → cập nhật tồn kho, tạo `HoaDon`, `ChiTietHoaDon`, topping và lịch sử điểm → commit → xóa giỏ hàng.
2. **Admin xác nhận/giao/hoàn thành:** Admin → Razor Orders Index → `OrdersController` → `OrderManagementService` kiểm tra chuyển trạng thái → `CoffeeShopDbContext`/SQL Server cập nhật `TrangThai`, `NguoiXuLy`, `NgayCapNhat` trong transaction → trả thông báo kết quả.
3. **Admin hủy:** Admin → modal nhập lý do → POST `CancelOrder` + anti-forgery token → validate `CancelOrderRequestModel` → `OrderManagementService` mở transaction `Serializable`, kiểm tra đơn và trạng thái → cộng lại số lượng từng sản phẩm/topping theo chi tiết đơn → ghi `Đã hủy`, lý do và người xử lý → commit. Nếu thiếu sản phẩm hoặc thao tác DB lỗi, transaction rollback và lỗi được truyền lên để xử lý; nếu trạng thái không hợp lệ, không thay đổi dữ liệu.

## Phân lớp và đối tượng

| Lớp | Vai trò |
|---|---|
| Boundary | Razor Views: `Checkout/Index`, `Admin/Orders/Index` (bao gồm modal), `Admin/Orders/Details` |
| Controller | `CheckoutController`, `Areas/Admin/OrdersController`: nhận request, xác thực ModelState, chọn View/điều hướng và thông báo |
| Business logic | `OrderManagementService`: quy tắc chuyển trạng thái, tính tồn kho cần hoàn, giao dịch xác nhận/hủy |
| Data access | `CoffeeShopDbContext`, EF Core và SQL Server `dbo.HoaDon`, `dbo.ChiTietHoaDon`, `dbo.ChiTietHoaDon_Topping`, `dbo.SanPham` |
| ViewModel | `OrderListViewModel`, `OrderDetailViewModel`, `CancelOrderRequestModel`: dữ liệu tường minh giữa Boundary và Controller |
