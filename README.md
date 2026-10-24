# Coffee Shop - Hệ Thống Bán Cà Phê & CRM (C# ASP.NET Core MVC & Microsoft SQL Server)

Dự án Coffee Blend được xây dựng và chuẩn hóa trên nền tảng **C# ASP.NET Core MVC (.NET 8 LTS)** kết hợp với **Microsoft SQL Server (MSSQL)** và **Entity Framework Core**.

---

## 1. Cấu Trúc Dự Án Chuẩn Mực

```text
c:\Xampp\htdocs\Coffee_Shop\
├── Areas/                               # Phân hệ Quản trị viên (Admin Area)
│   └── Admin/
│       ├── Controllers/
│       │   └── AdminControllers.cs      # Dashboard, Quản lý Đơn hàng, Sản phẩm
│       └── Views/
│           ├── Dashboard/Index.cshtml   # Thống kê doanh thu, số liệu kinh doanh
│           ├── Orders/Index.cshtml      # Tra cứu & cập nhật trạng thái đơn hàng
│           ├── Products/Index.cshtml    # Quản lý thực đơn, biến thể size M/L & giá bán
│           └── Shared/_AdminLayout.cshtml
├── Controllers/                         # Các Controller chính của Storefront
│   ├── HomeController.cs                # Trang chủ, Giới thiệu, Dịch vụ, Bài viết, Liên hệ
│   ├── ShopController.cs                # Thực đơn, bộ lọc danh mục, chi tiết món uống
│   ├── CartController.cs                # Giỏ hàng lưu bền vững, cập nhật số lượng, Voucher
│   ├── CheckoutController.cs            # Đặt hàng, chi tiết đơn, tích điểm thành viên
│   ├── AccountController.cs             # Đăng nhập, đăng ký, hồ sơ tích điểm CRM
│   └── CrmControllers.cs                # Đánh giá món uống & khảo sát ý kiến khách hàng
├── Data/
│   └── CoffeeShopDbContext.cs           # Entity Framework Core kết nối SQL Server
├── Helpers/
│   └── SessionExtensions.cs             # Tiện ích Serialize/Deserialize Object trong Session
├── Models/
│   ├── Entities/                        # Ánh xạ CSDL (LoaiSanPham, SanPham, DonHang...)
│   └── ViewModels/                      # ViewModels cho Storefront & Admin
├── Views/                               # Giao diện Razor Views (.cshtml)
│   ├── Account/                         # Đăng nhập, đăng ký, thông tin tài khoản
│   ├── Cart/                            # Trang giỏ hàng và voucher
│   ├── Checkout/                        # Thanh toán đơn hàng & xác nhận
│   ├── Feedback/                        # Đánh giá dịch vụ & món uống
│   ├── Home/                            # Index, About, Services, Blog, BlogSingle, Contact
│   ├── Shop/                            # Danh sách sản phẩm & chi tiết món uống
│   ├── Survey/                          # Khảo sát khách hàng trực tuyến
│   └── Shared/
│       └── _Layout.cshtml               # Layout giao diện chuẩn Responsive
├── wwwroot/                             # Tài nguyên tĩnh phục vụ trình duyệt
│   ├── admin/                           # CSS, JS dành riêng cho Admin Area
│   ├── css/                             # Stylesheets (Bootstrap, Flaticon, Icomoon, Custom)
│   ├── fonts/                           # Bộ fonts icon và chữ
│   ├── images/                          # Ảnh nền, banner, hình minh họa
│   ├── js/                              # Script tương tác, giỏ hàng AJAX, hiệu ứng
│   ├── picture/                         # Ảnh sản phẩm đồ uống và bánh ngọt
│   └── scss/                            # Source SCSS nếu cần tùy biến style
├── database/
│   ├── coffee_shop_mssql.sql            # Script khởi tạo CSDL & 48 sản phẩm mẫu trên SQL Server
│   └── Coffee_Shop_DB.sql               # Backup CSDL MySQL gốc
├── appsettings.json                     # Chuỗi kết nối CSDL và cấu hình ứng dụng
├── CoffeeShop.csproj                    # File định nghĩa dự án .NET 8
└── Program.cs                           # Cấu hình DI, Session, Cookie Authentication, Routes
```

---

## 2. Hướng Dẫn Chạy Dự Án

### Yêu cầu môi trường
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (LocalDB, SQL Express hoặc SQL Server Developer)

### Khởi chạy bằng dòng lệnh (Terminal / PowerShell)
Tại thư mục dự án:
```powershell
dotnet run
```
Mở trình duyệt truy cập:
👉 **http://localhost:5160**

---

## 3. Tài Khoản Đăng Nhập Mẫu

| Tài khoản | Tên đăng nhập | Mật khẩu | Quyền hạn / Chức năng |
|---|---|---|---|
| **Quản trị viên** | `admin` | `admin123` | Quản trị toàn bộ hệ thống (`/Admin`) |
| **Khách hàng** | `khachhang` | `123456` | Đặt món, tích điểm thành viên, đánh giá |

---

## 4. Các Tính Năng Nổi Bật

1. **Giao diện Storefront hoàn chỉnh**: Tương thích tốt trên cả máy tính và điện thoại.
2. **Đặt món linh hoạt**: Chọn kích cỡ (Size M, L), topping đi kèm, tính giá động theo thời gian thực.
3. **Giỏ hàng lưu bền vững & Voucher**: Giỏ hàng được lưu trong SQL Server tối đa 30 ngày; giỏ của khách vãng lai chỉ hiện khi chưa đăng nhập, còn giỏ thành viên được khôi phục khi đăng nhập lại và không hiển thị khi đăng xuất. Giỏ được xóa sau khi đặt hàng thành công. Áp dụng mã giảm giá như `COFFEE10` (giảm 10%), `GIAM30K` (giảm 30.000đ).
4. **CRM & Tích điểm thành viên**: Tự động tích lũy điểm khi hoàn tất đơn hàng, tự động thăng hạng hội viên.
5. **Trang Quản trị trực quan**: Dashboard thống kê doanh thu, cập nhật tiến độ đơn hàng và trạng thái món.
