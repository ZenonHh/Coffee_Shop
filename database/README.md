# Hướng Dẫn Cơ Sở Dữ Liệu (Microsoft SQL Server)

Dự án Coffee Blend sử dụng cơ sở dữ liệu **Microsoft SQL Server (MSSQL)** kết hợp với **Entity Framework Core 8.0**.

## 1. Kết Nối CSDL
- Cấu hình chuỗi kết nối nằm tại [`appsettings.json`](../appsettings.json):
  ```json
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=CoffeeShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```
  *(Lưu ý: Nếu dùng SQLEXPRESS thì đổi Server thành `.\\SQLEXPRESS`, nếu dùng LocalDB thì `(localdb)\\MSSQLLocalDB`)*

## 2. Khởi Tạo Cơ Sở Dữ Liệu (File Duy Nhất Chia Sẻ Cho Cả Nhóm)
Chỉ cần chạy **1 file duy nhất**: [`CoffeeShopDBNEW.sql`](CoffeeShopDBNEW.sql)
- Đã gộp toàn bộ **33 bảng**, quan hệ khóa ngoại (Foreign Keys), biến thể size M/L, giỏ hàng, vouchers, liên hệ, quản lý kho, tài khoản admin/nhân viên và 48 sản phẩm mẫu đầy đủ.
- Tự động tạo `CoffeeShopDb` nếu chưa có mà không bị lỗi đường dẫn cố định.
- Chạy được nhiều lần (idempotent) mà không bị xung đột khóa hoặc bảng đã tồn tại.

Cách chạy:
1. Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio.
2. Mở file [`CoffeeShopDBNEW.sql`](CoffeeShopDBNEW.sql) và nhấn **Execute (F5)**.
3. Hoặc chạy qua terminal `sqlcmd`:
   ```powershell
   sqlcmd -S "localhost" -E -C -i "database\CoffeeShopDBNEW.sql"
   ```

## 3. Tài Khoản Quản Trị Mẫu
- **Admin**: `admin` / Mật khẩu: `123456` (Vai trò: Quản trị viên)
- **Khách hàng**: `user01` / Mật khẩu: `123456`

