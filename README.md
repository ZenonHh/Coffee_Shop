# Coffee Shop - Hệ Thống Bán Cà Phê & CRM (C# ASP.NET Core MVC & Microsoft SQL Server)

Dự án Coffee Blend được xây dựng và chuẩn hóa trên nền tảng **C# ASP.NET Core MVC (.NET 8 LTS)** kết hợp với **Microsoft SQL Server (MSSQL)** và **Entity Framework Core**.

> 📌 **Tra cứu cấu trúc chi tiết & Ánh xạ Route/Controller/View**: Xem tại [`docs/PROJECT_MAP.md`](docs/PROJECT_MAP.md).

---

## 1. Cấu Trúc Dự Án Chuẩn Mực

```text
c:\Xampp\htdocs\Coffee_Shop\
├── Areas/                               # Phân hệ Quản trị viên (Admin Area)
│   └── Admin/
│       ├── Controllers/                 # 10 Controller nghiệp vụ Admin (1 file = 1 controller)
│       │   ├── AccountController.cs     # Đăng nhập / Đăng xuất trang quản trị
│       │   ├── CustomerCareController.cs# Chăm sóc khách hàng (Tab 1 Khảo sát, Tab 2 Hộp thư góp ý)
│       │   ├── DashboardController.cs   # Thống kê doanh thu, số liệu kinh doanh
│       │   ├── NhaCungCapController.cs  # Quản lý Nhà cung cấp & Phiếu nhập
│       │   ├── OrdersController.cs      # Tra cứu, xác nhận thanh toán & duyệt đơn hàng
│       │   ├── PhieuNhapController.cs   # Lập & xem chi tiết phiếu nhập kho
│       │   ├── ProductsController.cs    # Quản lý thực đơn, biến thể size M/L & giá bán
│       │   ├── SurveysController.cs     # Điều hướng đợt khảo sát
│       │   ├── TaiKhoanController.cs    # Phân quyền tài khoản nhân viên & khách hàng
│       │   └── VouchersController.cs    # Quản lý mã ưu đãi, voucher giảm giá
│       └── Views/                       # Giao diện quản trị tương ứng từng Controller
├── Controllers/                         # 8 Controller chính của Storefront (1 file = 1 controller)
│   ├── AccountController.cs             # Đăng nhập, đăng ký, hồ sơ tích điểm CRM
│   ├── CartController.cs                # Giỏ hàng lưu bền vững, cập nhật số lượng, Voucher
│   ├── CheckoutController.cs            # Đặt hàng, chi tiết đơn, tự động xác nhận khi thanh toán
│   ├── FeedbackController.cs            # Đánh giá món uống & phản hồi sản phẩm
│   ├── HomeController.cs                # Trang chủ, Giới thiệu, Dịch vụ, Bài viết, Liên hệ
│   ├── PromotionsController.cs          # Ưu đãi vouchers, hạng thành viên, đổi điểm nhận quà
│   ├── ShopController.cs                # Thực đơn, bộ lọc danh mục, chi tiết món uống
│   └── SurveyController.cs              # Khảo sát khách hàng trực tuyến, tích +50 điểm thưởng
├── Data/
│   └── CoffeeShopDbContext.cs           # Entity Framework Core kết nối SQL Server
├── Helpers/
│   ├── PersistentCartStore.cs           # Lưu trữ giỏ hàng độc lập
│   └── SessionExtensions.cs             # Tiện ích Serialize/Deserialize Session
├── Models/
│   ├── Entities/                        # Ánh xạ CSDL (SanPham, LoaiSanPham, HoaDon, KhachHang...)
│   └── ViewModels/                      # ViewModels cho Storefront & Admin
├── Services/
│   └── OrderManagementService.cs        # Xử lý trạng thái đơn hàng & tồn kho
├── Views/                               # Giao diện Razor Views (.cshtml) của Storefront
│   ├── Account/                         # Đăng nhập, đăng ký, hồ sơ khách hàng
│   ├── Cart/                            # Trang giỏ hàng và voucher
│   ├── Checkout/                        # Thanh toán đơn hàng & xác nhận
│   ├── Feedback/                        # Đánh giá dịch vụ & món uống
│   ├── Home/                            # Index, Blog, BlogSingle, Contact, Privacy
│   ├── Promotions/                      # Ưu đãi, bảng hạng thành viên, đổi điểm thưởng
│   ├── Shop/                            # Danh sách sản phẩm & chi tiết món uống
│   ├── Survey/                          # Khảo sát khách hàng trực tuyến
│   └── Shared/
│       └── _Layout.cshtml               # Layout giao diện chuẩn Responsive
├── wwwroot/                             # Tài nguyên tĩnh phục vụ trình duyệt
│   ├── admin/                           # CSS, JS dành riêng cho Admin Area
│   ├── css/                             # Stylesheets (coffee-blend-theme.css đồng bộ)
│   ├── fonts/                           # Bộ fonts icon và chữ
│   ├── images/                          # Ảnh nền, banner, hình minh họa
│   ├── js/                              # Script tương tác, giỏ hàng AJAX, hiệu ứng
│   └── picture/                         # Ảnh sản phẩm đồ uống và bánh ngọt
├── database/
│   ├── coffee_shop_mssql.sql            # Script khởi tạo CSDL & 48 sản phẩm mẫu trên SQL Server
│   ├── migrations/                      # Các bản cập nhật CSDL
│   └── README.md                        # Hướng dẫn thiết lập database
├── docs/
│   ├── PROJECT_MAP.md                   # BẢN ĐỒ CHI TIẾT DỰ ÁN & TRA CỨU CODE
│   └── order-workflow.md                # Quy trình vòng đời đơn hàng
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
| **Khách hàng** | `khachhang` | `123456` | Đặt món, tích điểm thành viên, đánh giá, đổi ưu đãi |

---

## 4. Các Tính Năng Nổi Bật

1. **Giao diện Coffee Blend sang trọng**: Đồng bộ tone màu đen huyền bí & vàng đồng trên toàn bộ các trang.
2. **Đặt món linh hoạt**: Chọn kích cỡ (Size M, L), topping đi kèm, tính giá động theo thời gian thực.
3. **Thanh toán & Tự động xác nhận đơn**: Đơn hàng khi khách thanh toán/chuyển khoản sẽ ngay lập tức được xác nhận, mã đơn thứ tự chuẩn `#HD0001`, `#HD0002`... kèm popup xem nhanh khi rê chuột.
4. **Chương trình Ưu đãi & Đổi điểm (`/Promotions`)**: Hiển thị ưu đãi hiện tại/sắp tới, giảm giá theo giá trị đơn, quyền lợi 4 hạng thành viên (Đồng, Bạc, Vàng, Kim Cương), cơ chế đổi điểm nhận voucher.
5. **Khảo sát ý kiến tích điểm (`/Survey`)**: Khách hàng hoàn thành khảo sát được tích ngay **+50 điểm** vào tài khoản.
6. **Chăm sóc khách hàng Admin (`/Admin/CustomerCare`)**: 2 tab điều hướng quản lý khảo sát và tiếp nhận hộp thư góp ý từ khách hàng.
