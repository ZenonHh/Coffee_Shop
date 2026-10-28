# BẢN ĐỒ DỰ ÁN & HƯỚNG DẪN TRA CỨU CODE (PROJECT MAP)
**Dự án**: Katinat Coffee - Hệ thống Bán Cà Phê, Quản lý Bán hàng & CRM Khách hàng
**Công nghệ**: C# ASP.NET Core (.NET 8 MVC), Entity Framework Core, Microsoft SQL Server.

---

## 1. Cấu Trúc Thư Mục Chuẩn Hóa

```text
Coffee_Shop/
├── Areas/                                 # Phân hệ Quản trị (Admin Area)
│   └── Admin/
│       ├── Controllers/                   # 10 Controller nghiệp vụ Admin (1 file = 1 controller)
│       │   ├── AccountController.cs       # Đăng nhập / Đăng xuất trang quản trị
│       │   ├── CustomerCareController.cs  # Chăm sóc khách hàng (Tab 1 Khảo sát, Tab 2 Hộp thư góp ý)
│       │   ├── DashboardController.cs     # Bảng điều khiển thống kê doanh thu, đơn hàng
│       │   ├── NhaCungCapController.cs    # Quản lý Nhà cung cấp & Phiếu nhập hàng
│       │   ├── OrdersController.cs        # Quản lý đơn hàng, duyệt đơn, xác nhận thanh toán
│       │   ├── PhieuNhapController.cs     # Lập & xem chi tiết phiếu nhập nguyên vật liệu
│       │   ├── ProductsController.cs      # Quản lý thực đơn đồ uống, biến thể size M/L
│       │   ├── SurveysController.cs       # Điều hướng & xử lý đợt khảo sát khách hàng
│       │   ├── TaiKhoanController.cs      # Quản lý tài khoản nhân viên & khách hàng
│       │   └── VouchersController.cs      # Quản lý mã ưu đãi, voucher giảm giá
│       └── Views/                         # Giao diện quản trị tương ứng từng Controller
│           ├── Account/Login.cshtml
│           ├── CustomerCare/Index.cshtml
│           ├── Dashboard/Index.cshtml
│           ├── NhaCungCap/Index.cshtml
│           ├── Orders/ (Index.cshtml, Details.cshtml)
│           ├── PhieuNhap/ (Index.cshtml, Create.cshtml, Details.cshtml)
│           ├── Products/Index.cshtml
│           ├── Shared/_AdminLayout.cshtml
│           ├── Surveys/Index.cshtml
│           ├── TaiKhoan/Index.cshtml
│           └── Vouchers/Index.cshtml
├── Controllers/                           # 8 Controller phân hệ Khách hàng (Storefront)
│   ├── AccountController.cs               # Đăng nhập, Đăng ký, Hồ sơ cá nhân & Lịch sử đơn
│   ├── CartController.cs                  # Giỏ hàng lưu bền vững (Persistent Cart), Áp Voucher
│   ├── CheckoutController.cs              # Thanh toán đơn hàng, Xác nhận thanh toán tự động
│   ├── FeedbackController.cs              # Đánh giá món uống, duyệt phản hồi
│   ├── HomeController.cs                  # Trang chủ, Giới thiệu, Blog tin tức, Liên hệ
│   ├── PromotionsController.cs            # Trang Ưu đãi, Hạng thành viên, Đổi điểm nhận quà
│   ├── ShopController.cs                  # Thực đơn sản phẩm, Bộ lọc loại món, Chi tiết món
│   └── SurveyController.cs                # Khảo sát ý kiến trực tuyến, Tích +50 điểm thưởng
├── Data/
│   └── CoffeeShopDbContext.cs             # EF Core DbContext, cấu hình DbSet & quan hệ bảng
├── Helpers/
│   ├── PersistentCartStore.cs             # Cơ chế lưu trữ giỏ hàng độc lập theo phiên
│   └── SessionExtensions.cs               # Tiện ích serialize/deserialize Session
├── Models/
│   ├── Entities/                          # Entity ánh xạ Database (Code-First / DB Mapped)
│   │   ├── CartSnapshot.cs                # Entity CoffeeShopBrowserCart (giỏ hàng tạm)
│   │   ├── CrmEntities.cs                 # Feedback, Survey, SurveyQuestion, SurveyAnswer, ContactMessage
│   │   ├── OrderEntities.cs               # Order (HoaDon), OrderDetail (ChiTietHoaDon)
│   │   ├── ProductEntities.cs             # Category (LoaiSanPham), Product (SanPham), ProductVariant (BienThe)
│   │   ├── PurchaseEntities.cs            # NhaCungCap, PhieuNhap, ChiTietPhieuNhap, Voucher
│   │   └── UserEntities.cs                # Account (TaiKhoan), Customer (KhachHang), PointHistory (LichSuDiem)
│   └── ViewModels/                        # ViewModels truyền dữ liệu sang View
│       ├── AccountViewModels.cs           # LoginViewModel, RegisterViewModel, CustomerProfileViewModel
│       ├── AdminViewModels.cs             # AdminDashboardViewModel, AdminProductListItemViewModel...
│       ├── ErrorViewModel.cs              # ErrorViewModel
│       ├── OrderViewModels.cs             # OrderListItemViewModel, AdminOrderDetailViewModel...
│       ├── PromotionsViewModel.cs         # PromotionsPageViewModel, TierBenefits, PointRedeemReward
│       └── ShopViewModels.cs              # ShopIndexViewModel, ProductDetailViewModel...
├── Services/
│   └── OrderManagementService.cs          # Logic trạng thái đơn hàng (OrderStatuses) & kiểm tra kho
├── Views/                                 # Giao diện khách hàng Storefront
│   ├── Account/ (Login, Register, Profile, AccessDenied)
│   ├── Cart/Index.cshtml
│   ├── Checkout/ (Index.cshtml, Success.cshtml)
│   ├── Feedback/Index.cshtml
│   ├── Home/ (Index, Blog, BlogSingle, Contact, Privacy)
│   ├── Promotions/Index.cshtml            # Giao diện Ưu đãi, Hạng thành viên, Đổi điểm
│   ├── Shared/ (_Layout.cshtml, Error.cshtml, _ValidationScriptsPartial.cshtml)
│   ├── Shop/ (Index.cshtml, Detail.cshtml)
│   └── Survey/Index.cshtml                # Giao diện khảo sát ý kiến +50 điểm
├── database/                              # Tài nguyên cơ sở dữ liệu
│   ├── coffee_shop_mssql.sql              # Script tạo CSDL & 48 sản phẩm mẫu trên SQL Server
│   ├── Coffee_Shop_DB.sql                 # Bản sao lưu gốc
│   ├── migrations/                        # Các script nâng cấp CSDL theo mốc thời gian
│   └── README.md                          # Hướng dẫn thiết lập SQL Server
├── docs/                                  # Tài liệu kỹ thuật
│   ├── PROJECT_MAP.md                     # Bản đồ tra cứu code (Tài liệu này)
│   └── order-workflow.md                  # Quy trình vòng đời đơn hàng
├── wwwroot/                               # Tài nguyên tĩnh trình duyệt
│   ├── admin/ (admin.css, admin.js)       # Style & script riêng của Admin
│   ├── css/ (coffee-blend-theme.css...)   # Toàn bộ CSS phong cách Katinat Coffee sang trọng
│   ├── fonts/                             # Icon fonts (Icomoon, Flaticon, Ionicons)
│   ├── images/                            # Ảnh nền, banner, decor
│   ├── js/                                # JavaScript tương tác giao diện
│   └── picture/                           # Kho ảnh món uống & bánh ngọt
├── appsettings.json                       # Chuỗi kết nối Database & thông số hệ thống
└── Program.cs                             # Khởi tạo DI, Cookie Auth, Session, Routing
```

---

## 2. Bảng Tra Cứu Nhanh: Route &rarr; Code Đối Soát

### A. Phân Hệ Người Dùng (Storefront)

| Chức Năng | Đường Dẫn (URL) | Controller & Action | View Hiển Thị | Model / Entity Liên Quan |
|---|---|---|---|---|
| **Trang chủ** | `/` hoặc `/Home` | `HomeController.Index` | `Views/Home/Index.cshtml` | `Product`, `Category` |
| **Thực đơn / Menu** | `/Shop` | `ShopController.Index` | `Views/Shop/Index.cshtml` | `ShopIndexViewModel` |
| **Chi tiết món uống** | `/Shop/Detail/{id}` | `ShopController.Detail` | `Views/Shop/Detail.cshtml` | `ProductDetailViewModel`, `ProductVariant` |
| **Giỏ hàng** | `/Cart` | `CartController.Index` | `Views/Cart/Index.cshtml` | `CartViewModel`, `Voucher` |
| **Đặt hàng & TT** | `/Checkout` | `CheckoutController.Index` | `Views/Checkout/Index.cshtml` | `CheckoutViewModel`, `Order` |
| **Đặt hàng thành công** | `/Checkout/Success/{id}` | `CheckoutController.Success` | `Views/Checkout/Success.cshtml` | `Order`, `OrderDetail` |
| **Chương trình Ưu đãi** | `/Promotions` | `PromotionsController.Index` | `Views/Promotions/Index.cshtml` | `PromotionsPageViewModel`, `Voucher`, `Customer` |
| **Đổi điểm nhận quà** | `POST /Promotions/RedeemPoints` | `PromotionsController.RedeemPoints` | (Xử lý & chuyển hướng) | `Customer`, `PointHistory`, `Voucher` |
| **Khảo sát ý kiến** | `/Survey` | `SurveyController.Index` | `Views/Survey/Index.cshtml` | `Survey`, `SurveyQuestion`, `SurveyAnswer` |
| **Gửi khảo sát (+50 điểm)**| `POST /Survey/SubmitSurvey` | `SurveyController.SubmitSurvey` | (Tích điểm & chuyển hướng) | `Customer`, `PointHistory` |
| **Đăng nhập khách** | `/Account/Login` | `AccountController.Login` | `Views/Account/Login.cshtml` | `LoginViewModel` |
| **Đăng ký khách** | `/Account/Register` | `AccountController.Register` | `Views/Account/Register.cshtml` | `RegisterViewModel` |
| **Hồ sơ & Đơn hàng** | `/Account/Profile` | `AccountController.Profile` | `Views/Account/Profile.cshtml` | `CustomerProfileViewModel`, `Order` |
| **Bài viết & Tin tức** | `/Home/Blog` | `HomeController.Blog` | `Views/Home/Blog.cshtml` | - |
| **Liên hệ & Góp ý** | `/Home/Contact` | `HomeController.Contact` | `Views/Home/Contact.cshtml` | `ContactMessage` |

---

### B. Phân Hệ Quản Trị (Admin Area - Quyền: Admin, Staff, NhanVien)

| Chức Năng | Đường Dẫn (URL) | Controller & Action | View Hiển Thị | Ghi Chú Nghiệp Vụ |
|---|---|---|---|---|
| **Bảng điều khiển** | `/Admin/Dashboard` | `DashboardController.Index` | `Areas/Admin/Views/Dashboard/Index.cshtml` | Thống kê doanh thu, số đơn, biểu đồ |
| **Quản lý Đơn hàng** | `/Admin/Orders` | `OrdersController.Index` | `Areas/Admin/Views/Orders/Index.cshtml` | Mã `#HD0001` thứ tự, Hover pop-up, Xác nhận TT |
| **Chi tiết đơn hàng** | `/Admin/Orders/Details/{id}` | `OrdersController.Details` | `Areas/Admin/Views/Orders/Details.cshtml` | Cập nhật tiến trình: Đang pha chế, Đang giao... |
| **Quản lý Thực đơn** | `/Admin/Products` | `ProductsController.Index` | `Areas/Admin/Views/Products/Index.cshtml` | Bật/tắt món uống, quản lý giá từng size |
| **Chăm sóc Khách hàng** | `/Admin/CustomerCare` | `CustomerCareController.Index` | `Areas/Admin/Views/CustomerCare/Index.cshtml` | Tab 1: Tạo đợt khảo sát; Tab 2: Hộp thư góp ý |
| **Nhà cung cấp & Kho** | `/Admin/NhaCungCap` | `NhaCungCapController.Index` | `Areas/Admin/Views/NhaCungCap/Index.cshtml` | Quản lý NCC & tích hợp xem phiếu nhập kho |
| **Lập Phiếu nhập** | `/Admin/PhieuNhap/Create` | `PhieuNhapController.Create` | `Areas/Admin/Views/PhieuNhap/Create.cshtml` | Nhập nguyên vật liệu từ nhà cung cấp |
| **Quản lý Vouchers** | `/Admin/Vouchers` | `VouchersController.Index` | `Areas/Admin/Views/Vouchers/Index.cshtml` | Tạo mã giảm giá %, số tiền, giới hạn dùng |
| **Quản lý Tài khoản** | `/Admin/TaiKhoan` | `TaiKhoanController.Index` | `Areas/Admin/Views/TaiKhoan/Index.cshtml` | Phân quyền Quản trị, Nhân viên, Khách hàng |
| **Đăng nhập Quản trị** | `/Admin/Account/Login` | `AccountController.Login` | `Areas/Admin/Views/Account/Login.cshtml` | Đăng nhập khu vực quản trị |

---

## 3. Bảng Ánh Xạ Entity Model & Database Table

| Entity C# Class | Tên Bảng SQL Server | File Khai Báo | Vai Trò |
|---|---|---|---|
| `Product` | `SanPham` | `Models/Entities/ProductEntities.cs` | Danh sách đồ uống, giá gốc, hình ảnh, tồn kho |
| `Category` | `LoaiSanPham` | `Models/Entities/ProductEntities.cs` | Phân loại đồ uống (Cà phê, Trà, Bánh...) |
| `ProductVariant` | `BienTheSanPham` | `Models/Entities/ProductEntities.cs` | Biến thể size M, L và giá bán tương ứng |
| `Order` | `HoaDon` | `Models/Entities/OrderEntities.cs` | Đơn hàng, người nhận, địa chỉ, phương thức TT |
| `OrderDetail` | `ChiTietHoaDon` | `Models/Entities/OrderEntities.cs` | Món uống trong đơn, size, số lượng, đơn giá |
| `Customer` | `KhachHang` | `Models/Entities/UserEntities.cs` | Khách hàng, điểm tích lũy, hạng TV (Đồng/Bạc/Vàng/KC) |
| `Account` | `TaiKhoan` | `Models/Entities/UserEntities.cs` | Tài khoản đăng nhập hệ thống, vai trò phân quyền |
| `PointHistory` | `LichSuDiem` | `Models/Entities/UserEntities.cs` | Lịch sử cộng/trừ điểm (đổi voucher, làm khảo sát) |
| `Voucher` | `Voucher` | `Models/Entities/PurchaseEntities.cs` | Mã khuyến mãi, % hoặc tiền giảm, điều kiện áp dụng |
| `NhaCungCap` | `NhaCungCap` | `Models/Entities/PurchaseEntities.cs` | Thông tin đơn vị cung ứng hạt cafe, nguyên liệu |
| `PhieuNhap` | `PhieuNhap` | `Models/Entities/PurchaseEntities.cs` | Phiếu ghi nhận nhập hàng vào kho |
| `ChiTietPhieuNhap` | `ChiTietPhieuNhap` | `Models/Entities/PurchaseEntities.cs` | Chi tiết mặt hàng và số lượng nhập |
| `Survey` | `KhaoSat` | `Models/Entities/CrmEntities.cs` | Đợt khảo sát ý kiến khách hàng |
| `SurveyQuestion` | `CauHoiKhaoSat` | `Models/Entities/CrmEntities.cs` | Các câu hỏi trong đợt khảo sát |
| `SurveyAnswer` | `CauTraLoiKhaoSat` | `Models/Entities/CrmEntities.cs` | Câu trả lời của khách hàng |
| `ContactMessage` | `TinNhanLienHe` | `Models/Entities/CrmEntities.cs` | Lời nhắn gửi từ trang Liên hệ |
| `Feedback` | `PhanHoi` | `Models/Entities/CrmEntities.cs` | Đánh giá sao & nhận xét sản phẩm |
