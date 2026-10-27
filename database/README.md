# Hướng Dẫn Cơ Sở Dữ Liệu (Microsoft SQL Server)

Dự án Coffee Blend sử dụng cơ sở dữ liệu **Microsoft SQL Server (MSSQL)** kết hợp với **Entity Framework Core 8.0**.

## 1. Kết Nối CSDL
- Cấu hình chuỗi kết nối nằm tại [`appsettings.json`](../appsettings.json):
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```

## 2. Khởi Tạo Cơ Sở Dữ Liệu
1. Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio.
2. Tạo database: `CREATE DATABASE CoffeeShopDB;`
3. Chạy script chính: [`coffee_shop_mssql.sql`](coffee_shop_mssql.sql) để tạo toàn bộ bảng, khóa chính, khóa ngoại và 48 sản phẩm mẫu chuẩn.

## 3. Các Script Cập Nhật (migrations/)
Các file trong thư mục `migrations/` ghi nhận tiến trình nâng cấp schema:
- `sync_database.sql`: Đồng bộ schema mới nhất (Voucher, ContactMessage, Điểm tích lũy, Khảo sát, Hạng thành viên).
- `20261025_order_workflow.sql`: Cập nhật quy trình đơn hàng, địa chỉ giao, người xử lý.
- `20261024_inventory_purchases_vouchers.sql`: Quản lý kho, nhà cung cấp, phiếu nhập, vouchers.
- `20261024_browser_cart.sql`: Giỏ hàng lưu tạm cho khách duyệt web.
- `20261013_staff_accounts.sql`: Phân quyền tài khoản nhân viên & quản trị.
- `20261008_crm_surveys.sql`: Khảo sát ý kiến và phản hồi khách hàng.
- `20261008_product_variants.sql`: Biến thể size M/L và giá bán từng size.
- `20261006_customer_management.sql`: Quản lý khách hàng và điểm tích lũy.
