# BẢN ĐỒ DỰ ÁN & HƯỚNG DẪN TRA CỨU CODE (PROJECT MAP)
**Dự án**: Katinat Coffee - Hệ thống Bán Cà Phê, Quản lý Bán hàng & CRM Khách hàng
**Công nghệ**: C# ASP.NET Core (.NET 8 MVC), Entity Framework Core, Microsoft SQL Server.

---

## 1. Cấu Trúc Thư Mục Chuẩn Hóa

```text
Coffee_Shop/
├── Presentation/
│   ├── Areas/Admin/                       # Admin controllers và Razor views
│   ├── Controllers/                       # Storefront controllers và API
│   └── Views/                             # Razor views khách hàng
├── Business/
│   ├── Services/                          # Nghiệp vụ dùng chung, quản lý đơn, báo cáo
│   └── ViewModels/                        # ViewModels website/Admin
├── Data/
│   ├── Entities/                          # Entity ánh xạ các bảng database
│   ├── Migrations/                        # EF Core migrations
│   └── CoffeeShopDbContext.cs             # DbContext, DbSet và quan hệ bảng
├── Helpers/
│   ├── PersistentCartStore.cs             # Cơ chế lưu trữ giỏ hàng độc lập theo phiên
│   └── SessionExtensions.cs               # Tiện ích serialize/deserialize Session
├── CoffeeShop.Mobile/                     # Ứng dụng .NET MAUI dùng backend/API
├── wwwroot/                               # Tài nguyên tĩnh trình duyệt
├── database/                              # Script SQL và hướng dẫn database
├── docs/                                  # Bản đồ dự án và tài liệu nghiệp vụ
├── appsettings.json                       # Cấu hình ứng dụng
├── CoffeeShop.csproj                      # Backend MVC/API .NET 8
└── Program.cs                             # Khởi tạo DI, middleware và routing
```

---

## 2. Bảng Tra Cứu Nhanh: Route &rarr; Code Đối Soát

### A. Phân Hệ Người Dùng (Storefront)

| Chức Năng | Đường Dẫn (URL) | Controller & Action | View Hiển Thị | Model / Entity Liên Quan |
|---|---|---|---|---|
| **Trang chủ** | `/` hoặc `/Home` | `HomeController.Index` | `Presentation/Views/Home/Index.cshtml` | `Product`, `Category` |
| **Thực đơn / Menu** | `/Shop` | `ShopController.Index` | `Presentation/Views/Shop/Index.cshtml` | `ShopIndexViewModel` |
| **Chi tiết món uống** | `/Shop/Detail/{id}` | `ShopController.Detail` | `Presentation/Views/Shop/Detail.cshtml` | `ProductDetailViewModel`, `ProductVariant` |
| **Giỏ hàng** | `/Cart` | `CartController.Index` | `Presentation/Views/Cart/Index.cshtml` | `CartViewModel`, `Voucher` |
| **Đặt hàng & TT** | `/Checkout` | `CheckoutController.Index` | `Presentation/Views/Checkout/Index.cshtml` | `CheckoutViewModel`, `Order` |
| **Đặt hàng thành công** | `/Checkout/Success/{id}` | `CheckoutController.Success` | `Presentation/Views/Checkout/Success.cshtml` | `Order`, `OrderDetail` |
| **Chương trình Ưu đãi** | `/Promotions` | `PromotionsController.Index` | `Presentation/Views/Promotions/Index.cshtml` | `PromotionsPageViewModel`, `Voucher`, `Customer` |
| **Đổi điểm nhận quà** | `POST /Promotions/RedeemPoints` | `PromotionsController.RedeemPoints` | (Xử lý & chuyển hướng) | `Customer`, `PointHistory`, `Voucher` |
| **Khảo sát ý kiến** | `/Survey` | `SurveyController.Index` | `Presentation/Views/Survey/Index.cshtml` | `Survey`, `SurveyQuestion`, `SurveyAnswer` |
| **Gửi khảo sát (+50 điểm)**| `POST /Survey/SubmitSurvey` | `SurveyController.SubmitSurvey` | (Tích điểm & chuyển hướng) | `Customer`, `PointHistory` |
| **Đăng nhập khách** | `/Account/Login` | `AccountController.Login` | `Presentation/Views/Account/Login.cshtml` | `LoginViewModel` |
| **Đăng ký khách** | `/Account/Register` | `AccountController.Register` | `Presentation/Views/Account/Register.cshtml` | `RegisterViewModel` |
| **Hồ sơ & Đơn hàng** | `/Account/Profile` | `AccountController.Profile` | `Presentation/Views/Account/Profile.cshtml` | `CustomerProfileViewModel`, `Order` |
| **Bài viết & Tin tức** | `/Home/Blog` | `HomeController.Blog` | `Presentation/Views/Home/Blog.cshtml` | - |
| **Liên hệ & Góp ý** | `/Home/Contact` | `HomeController.Contact` | `Presentation/Views/Home/Contact.cshtml` | `ContactMessage` |

---

### B. Phân Hệ Quản Trị (Admin Area - Quyền: Admin, Staff, NhanVien)

| Chức Năng | Đường Dẫn (URL) | Controller & Action | View Hiển Thị | Ghi Chú Nghiệp Vụ |
|---|---|---|---|---|
| **Bảng điều khiển** | `/Admin/Dashboard` | `DashboardController.Index` | `Presentation/Areas/Admin/Views/Dashboard/Index.cshtml` | Chỉ có thẻ tóm tắt và đường dẫn thống kê; thẻ bán hàng chỉ dành cho Admin |
| **Quản lý Đơn hàng** | `/Admin/Orders` | `OrdersController.Index` | `Presentation/Areas/Admin/Views/Orders/Index.cshtml` | Mã `#HD0001` thứ tự, Hover pop-up, Xác nhận TT |
| **Chi tiết đơn hàng** | `/Admin/Orders/Details/{id}` | `OrdersController.Details` | `Presentation/Areas/Admin/Views/Orders/Details.cshtml` | Cập nhật tiến trình: Đang pha chế, Đang giao... |
| **Quản lý Thực đơn** | `/Admin/Products` | `ProductsController.Index` | `Presentation/Areas/Admin/Views/Products/Index.cshtml` | Bật/tắt món uống, quản lý giá từng size |
| **Khách hàng** | `/Admin/KhachHang` | `KhachHangController.Index` | `Presentation/Areas/Admin/Views/KhachHang/Index.cshtml` | Lọc hồ sơ, trạng thái, hạng thẻ, nhóm tuổi và sở thích |
| **Thống kê CRM** | `/Admin/Statistics` | `StatisticsController.Index` | `Presentation/Areas/Admin/Views/Statistics/Index.cshtml` | Ba tab khách hàng, hài lòng và khảo sát; lọc thời gian |
| **Khảo sát** | `/Admin/Surveys` | `SurveysController.Index` | `Presentation/Areas/Admin/Views/Surveys/Index.cshtml` | Quản lý khảo sát, tổng hợp và từng câu trả lời |
| **Phản hồi** | `/Admin/Feedback` | `FeedbackController.Index` | `Presentation/Areas/Admin/Views/Feedback/Index.cshtml` | Theo dõi trạng thái và trả lời đánh giá/góp ý |
| **Nhà cung cấp & Kho** | `/Admin/NhaCungCap` | `NhaCungCapController.Index` | `Presentation/Areas/Admin/Views/NhaCungCap/Index.cshtml` | Quản lý NCC & tích hợp xem phiếu nhập kho |
| **Lập Phiếu nhập** | `/Admin/PhieuNhap/Create` | `PhieuNhapController.Create` | `Presentation/Areas/Admin/Views/PhieuNhap/Create.cshtml` | Nhập nguyên vật liệu từ nhà cung cấp |
| **Quản lý Vouchers** | `/Admin/Vouchers` | `VouchersController.Index` | `Presentation/Areas/Admin/Views/Vouchers/Index.cshtml` | Tạo mã giảm giá %, số tiền, giới hạn dùng |
| **Quản lý Tài khoản** | `/Admin/TaiKhoan` | `TaiKhoanController.Index` | `Presentation/Areas/Admin/Views/TaiKhoan/Index.cshtml` | Phân quyền Quản trị, Nhân viên, Khách hàng |
| **Đăng nhập Quản trị** | `/Admin/Account/Login` | `AccountController.Login` | `Presentation/Areas/Admin/Views/Account/Login.cshtml` | Đăng nhập khu vực quản trị |

---

## 3. Bảng Ánh Xạ Entity Model & Database Table

| Entity C# Class | Tên Bảng SQL Server | File Khai Báo | Vai Trò |
|---|---|---|---|
| `Product` | `SanPham` | `Data/Entities/ProductEntities.cs` | Danh sách đồ uống, giá gốc, hình ảnh, tồn kho |
| `Category` | `LoaiSanPham` | `Data/Entities/ProductEntities.cs` | Phân loại đồ uống (Cà phê, Trà, Bánh...) |
| `ProductVariant` | `BienTheSanPham` | `Data/Entities/ProductEntities.cs` | Biến thể size M, L và giá bán tương ứng |
| `Order` | `HoaDon` | `Data/Entities/OrderEntities.cs` | Đơn hàng, người nhận, địa chỉ, phương thức TT |
| `OrderDetail` | `ChiTietHoaDon` | `Data/Entities/OrderEntities.cs` | Món uống trong đơn, size, số lượng, đơn giá |
| `Customer` | `KhachHang` | `Data/Entities/UserEntities.cs` | Khách hàng, điểm tích lũy, hạng TV (Đồng/Bạc/Vàng/KC) |
| `Account` | `TaiKhoan` | `Data/Entities/UserEntities.cs` | Tài khoản đăng nhập hệ thống, vai trò phân quyền |
| `PointHistory` | `LichSuDiem` | `Data/Entities/UserEntities.cs` | Lịch sử cộng/trừ điểm (đổi voucher, làm khảo sát) |
| `Voucher` | `Voucher` | `Data/Entities/PurchaseEntities.cs` | Mã khuyến mãi, % hoặc tiền giảm, điều kiện áp dụng |
| `NhaCungCap` | `NhaCungCap` | `Data/Entities/PurchaseEntities.cs` | Thông tin đơn vị cung ứng hạt cafe, nguyên liệu |
| `PhieuNhap` | `PhieuNhap` | `Data/Entities/PurchaseEntities.cs` | Phiếu ghi nhận nhập hàng vào kho |
| `ChiTietPhieuNhap` | `ChiTietPhieuNhap` | `Data/Entities/PurchaseEntities.cs` | Chi tiết mặt hàng và số lượng nhập |
| `Survey` | `KhaoSat` | `Data/Entities/CrmEntities.cs` | Đợt khảo sát ý kiến khách hàng |
| `SurveyQuestion` | `CauHoiKhaoSat` | `Data/Entities/CrmEntities.cs` | Các câu hỏi trong đợt khảo sát |
| `SurveyAnswer` | `CauTraLoiKhaoSat` | `Data/Entities/CrmEntities.cs` | Câu trả lời của khách hàng |
| `ContactMessage` | `TinNhanLienHe` | `Data/Entities/CrmEntities.cs` | Lời nhắn gửi từ trang Liên hệ |
| `Feedback` | `PhanHoi` | `Data/Entities/CrmEntities.cs` | Đánh giá sao & nhận xét sản phẩm |
