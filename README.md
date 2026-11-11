# Coffee Shop

Hệ thống quản lý và bán hàng cho cửa hàng cà phê, gồm website khách hàng, khu vực quản trị, REST API và ứng dụng di động. Backend sử dụng ASP.NET Core MVC và SQL Server; mobile app được xây dựng bằng .NET MAUI.

## Mục lục

- [Tính năng](#tính-năng)
- [Công nghệ và yêu cầu](#công-nghệ-và-yêu-cầu)
- [Tải mã nguồn](#tải-mã-nguồn)
- [Cấu hình cơ sở dữ liệu](#cấu-hình-cơ-sở-dữ-liệu)
- [Chạy website và API](#chạy-website-và-api)
- [Chạy ứng dụng mobile](#chạy-ứng-dụng-mobile)
- [Tài khoản và quyền truy cập](#tài-khoản-và-quyền-truy-cập)
- [API di động](#api-di-động)
- [Cấu trúc repository](#cấu-trúc-repository)
- [Kiểm tra và xử lý sự cố](#kiểm-tra-và-xử-lý-sự-cố)
- [Tài liệu tham khảo](#tài-liệu-tham-khảo)

## Tính năng

### Website khách hàng

- Duyệt thực đơn, danh mục và chi tiết món; tùy chọn kích cỡ và topping.
- Quản lý giỏ hàng, áp dụng voucher và đặt hàng.
- Đăng ký, đăng nhập, cập nhật hồ sơ và xem lịch sử đơn.
- Chương trình thành viên, lịch sử điểm, đổi ưu đãi, đánh giá và khảo sát.

### Khu vực quản trị

- Dashboard và báo cáo hoạt động.
- Quản lý sản phẩm, danh mục, biến thể, tồn kho, nhà cung cấp và phiếu nhập.
- Quản lý đơn hàng, thanh toán, khách hàng, tài khoản, voucher, khảo sát và phản hồi.
- Phân quyền theo tài khoản và vai trò.

### Ứng dụng mobile

- Ứng dụng khách hàng Android/iOS dùng chung backend và REST API.
- Đăng nhập, xem thực đơn, tùy chỉnh món, đặt hàng, voucher, hồ sơ và lịch sử đơn.

Điểm từ đơn hàng được cộng khi đơn chuyển sang trạng thái **Hoàn thành**, không phải ngay khi khách đặt hàng. Khảo sát có thể có chính sách thưởng điểm riêng.

## Công nghệ và yêu cầu

| Thành phần | Công nghệ |
|---|---|
| Website, Admin và REST API | ASP.NET Core MVC, .NET 8 |
| ORM và dữ liệu | Entity Framework Core 8, Microsoft SQL Server |
| Mobile | .NET MAUI, .NET 10, CommunityToolkit.Maui |
| Nền tảng mobile | Android; iOS cần máy Mac và workload phù hợp |

Để chạy website/API:

- Git.
- .NET 8 SDK.
- SQL Server (LocalDB, SQL Server Express/Developer hoặc máy chủ tương thích).
- SSMS hoặc `sqlcmd` để khởi tạo database nếu dùng script SQL.

Để build mobile Android, cài thêm .NET 10 SDK, workload MAUI Android và Android SDK/emulator. Có thể kiểm tra workload bằng `dotnet workload list`.

## Tải mã nguồn

```powershell
git clone https://github.com/ZenonHh/Coffee_Shop.git
Set-Location Coffee_Shop
```

Nếu đã tải source dạng ZIP, giải nén rồi mở terminal tại thư mục chứa `CoffeeShop.csproj`.

## Cấu hình cơ sở dữ liệu

Ứng dụng đọc chuỗi kết nối `ConnectionStrings:DefaultConnection`; nếu không có thì đọc `ConnectionStrings:Default`. Cấu hình này phải trỏ tới SQL Server và database của dự án.

Trong môi trường phát triển, nên đặt connection string bằng User Secrets hoặc biến môi trường thay vì lưu mật khẩu vào Git:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=CoffeeShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Điều chỉnh `Server` và phương thức xác thực theo môi trường của bạn. SQL authentication có thể dùng `User Id` và `Password`; không đưa thông tin đó vào README, log hoặc commit.

### Khởi tạo schema và dữ liệu mẫu

Script SQL tổng hợp nằm tại [`database/CoffeeShopDBNEW.sql`](database/CoffeeShopDBNEW.sql). Có thể chạy bằng SSMS hoặc:

```powershell
sqlcmd -S "localhost" -E -C -i "database\CoffeeShopDBNEW.sql"
```

> **Cảnh báo:** script này có thao tác tạo lại/xóa dữ liệu bảng. Chỉ chạy trên database phát triển có thể khởi tạo lại; sao lưu trước và không chạy trên database đang có dữ liệu cần giữ.

Các script khác trong `database/` là bản SQL hoặc script nâng cấp theo từng giai đoạn. Kiểm tra [`database/README.md`](database/README.md) và nội dung script trước khi chạy để chọn đúng phương án cho database hiện có. Không chạy tất cả script một cách tùy ý.

## Chạy website và API

Từ thư mục gốc:

```powershell
dotnet restore CoffeeShop.csproj
dotnet run --project CoffeeShop.csproj --launch-profile http
```

Profile `http` chạy ở `http://localhost:5160`. Mở địa chỉ đó để vào website. Khu vực quản trị nằm dưới `/Admin` (trang đăng nhập thường là `/Admin/Account/Login`). Health check: `/health`.

Nếu chạy profile `https`, URL mặc định được khai báo trong `Properties/launchSettings.json`. Dùng chứng chỉ phát triển HTTPS đã được tin cậy trên máy:

```powershell
dotnet dev-certs https --trust
dotnet run --project CoffeeShop.csproj --launch-profile https
```

API mobile được host cùng ứng dụng web, tại các đường dẫn `/api/...`.

## Chạy ứng dụng mobile

Mobile là project riêng tại `CoffeeShop.Mobile/`; không chạy bằng lệnh `dotnet run` của website.

Khôi phục workload/dependencies (nếu máy chưa cài workload MAUI cần thiết):

```powershell
dotnet workload restore CoffeeShop.Mobile\CoffeeShop.Mobile.csproj
dotnet restore CoffeeShop.Mobile\CoffeeShop.Mobile.csproj
```

Build Android:

```powershell
 dotnet build .\CoffeeShop.Mobile\CoffeeShop.Mobile.csproj -t:Run -f net10.0-android
```

Có thể mở repository bằng Visual Studio có hỗ trợ .NET MAUI, chọn project `CoffeeShop.Mobile`, chọn Android Emulator/device rồi chạy. APK đầu ra thường nằm trong `CoffeeShop.Mobile/bin/Debug/net10.0-android/`.

Backend phải đang chạy và truy cập được từ thiết bị:

- Android Emulator: địa chỉ API mặc định là `http://10.0.2.2:5160/` trong `CoffeeShop.Mobile/MauiProgram.cs`; `10.0.2.2` trỏ về máy host.
- Điện thoại thật: thay địa chỉ API bằng hostname/IP LAN của máy chạy backend và cấu hình network/firewall phù hợp. Điện thoại và máy chủ cần cùng mạng.
- iOS Simulator/device: cần máy Mac có Xcode và workload iOS. Cấu hình endpoint phải trỏ tới địa chỉ có thể truy cập từ simulator/device.

HTTP được bật cho môi trường phát triển cục bộ; không dùng HTTP/cleartext cho bản phát hành. Với deployment, dùng HTTPS và cấu hình endpoint an toàn phù hợp.

## Tài khoản và quyền truy cập

- Khách hàng có thể đăng ký từ website hoặc ứng dụng mobile.
- Muốn đăng nhập Admin, cần tài khoản có vai trò quản trị đã được tạo trong database. Trang đăng nhập quản trị nằm dưới `/Admin`.
- Database/script mẫu có thể chứa một số tài khoản seed. Tên đăng nhập, mật khẩu và trạng thái tài khoản phụ thuộc script/database bạn sử dụng; hãy kiểm tra dữ liệu seed hoặc tạo tài khoản phù hợp. Không dựa vào thông tin đăng nhập cũ trong tài liệu chưa được xác minh.
- Không sử dụng mật khẩu mẫu trong môi trường thật; đổi hoặc vô hiệu hóa tài khoản seed trước khi triển khai.

## API di động

Các route API được triển khai trong `Controllers/Api/`. Một số endpoint chính:

| Method | Đường dẫn | Mục đích |
|---|---|---|
| `POST` | `/api/auth/login` | Đăng nhập |
| `POST` | `/api/auth/register` | Đăng ký khách hàng |
| `GET` | `/api/auth/profile` | Hồ sơ hiện tại |
| `GET` | `/api/auth/points` | Lịch sử điểm |
| `GET` | `/api/menu/categories` | Danh mục món |
| `GET` | `/api/menu/products` | Danh sách/tìm món |
| `GET` | `/api/menu/product-detail/{id}` | Chi tiết món và lựa chọn |
| `GET` | `/api/voucher` | Voucher |
| `POST` | `/api/order/checkout` | Đặt hàng |
| `GET` | `/api/order/history` | Lịch sử đơn |

Các request mẫu nằm trong [`CoffeeShop.Api.http`](CoffeeShop.Api.http). Endpoint có yêu cầu xác thực cần access token nhận được từ API đăng nhập. Token API được lưu trong cache của tiến trình backend, có thời hạn tối đa 7 ngày và sẽ mất hiệu lực khi backend khởi động lại. API không tự chạy migration database.

## Cấu trúc repository

```text
Coffee_Shop/
├── Areas/Admin/               # Controllers và Views quản trị
├── Controllers/               # Website khách hàng và Controllers/Api
├── CoffeeShop.Mobile/         # Ứng dụng .NET MAUI
│   ├── Views/                 # Trang XAML và code-behind
│   ├── ViewModels/            # Trạng thái và lệnh UI
│   ├── Services/              # API và dịch vụ mobile
│   └── Resources/             # Styles, màu sắc, ảnh
├── Data/                      # DbContext
├── Helpers/                   # Session, giỏ hàng và helpers
├── Migrations/                # EF Core migrations
├── Models/                    # Entity và ViewModel
├── Services/                  # Nghiệp vụ backend, quản lý đơn
├── Views/                     # Razor Views website khách hàng
├── wwwroot/                   # CSS, JavaScript, ảnh và tài nguyên web
├── database/                  # SQL khởi tạo và các script database
├── docs/                      # Tài liệu kỹ thuật/nghiệp vụ
├── CoffeeShop.csproj          # Backend web/API (.NET 8)
└── Program.cs                 # DI, middleware và routing
```

## Kiểm tra và xử lý sự cố

Build backend:

```powershell
dotnet build CoffeeShop.csproj
```

Build Android:

```powershell
dotnet build CoffeeShop.Mobile\CoffeeShop.Mobile.csproj --framework net10.0-android
```

Các điểm cần kiểm tra nếu không chạy được:

1. SQL Server đang hoạt động, database đã được khởi tạo và connection string trỏ đúng server/database.
2. Backend đang lắng nghe đúng URL/port; mở `/health` để kiểm tra ứng dụng và kết nối database.
3. Mobile trỏ tới endpoint mà emulator/device truy cập được; địa chỉ `localhost` trên điện thoại không phải máy tính đang chạy backend.
4. Kiểm tra output của `dotnet build`, terminal đang chạy backend và Android Logcat/emulator khi có lỗi.
5. Nếu schema không khớp, xem các migration trong `Migrations/` và hướng dẫn tại `database/README.md`; sao lưu trước mọi thao tác cập nhật dữ liệu.

## Tài liệu tham khảo

- [Bản đồ project và tra cứu code](docs/PROJECT_MAP.md)
- [Quy trình đơn hàng](docs/order-workflow.md)
- [Hướng dẫn database](database/README.md)
- [HTTP request mẫu cho API](CoffeeShop.Api.http)

## Bản quyền và thư viện

Các package và thư viện bên thứ ba tuân theo giấy phép riêng. Tính năng xuất PDF dùng QuestPDF; kiểm tra điều kiện giấy phép QuestPDF hiện hành và bảo đảm loại hình sử dụng của bạn tuân thủ trước khi triển khai thương mại.
