# Coffee Shop

Website Coffee Blend chạy trên PHP/XAMPP với MySQL.

## Cấu trúc dự án

```text
/
├── admin/
│   ├── AdminController.php # Xác thực, điều phối trang và thao tác quản trị
│   ├── AdminModel.php      # Truy vấn và thao tác dữ liệu quản trị
│   ├── admin.css           # Giao diện quản trị
│   ├── admin.js            # Tương tác phía trình duyệt
│   └── index.php           # Giao diện quản trị hợp nhất
├── database/
│   ├── migrations/         # Migration theo thứ tự ngày tháng
│   ├── connect.php         # Adapter MySQLi cho model tương thích
│   └── coffee_shop_db.sql  # Schema và dữ liệu khởi tạo MySQL
├── includes/               # Helper và partial dùng chung cho các trang
├── src/                    # Model và thành phần giao diện PHP dùng chung
├── css/, js/, fonts/        # Tài nguyên giao diện tĩnh
├── images/                  # Ảnh giao diện và ảnh dự phòng
├── picture/                 # Ảnh món ăn/sản phẩm
├── *.php                    # Endpoint storefront, tài khoản và CRM khách hàng
├── connect.php              # Kết nối PDO storefront/CRM
└── config.local.php         # Cấu hình cục bộ tùy chọn
```

Các trang PHP ở thư mục gốc là những endpoint được trình duyệt gọi trực tiếp. `includes/` gom các helper và partial dùng chung; `src/` giữ model, truy cập dữ liệu kiểu ứng dụng và thành phần giao diện được include bởi các trang. Kết nối MySQL và trang quản trị được giữ riêng theo cấu trúc dự án.

Giao diện quản trị đang được phục vụ tập trung qua `admin/index.php`; các trang quản trị cũ trong những thư mục con đã được loại bỏ. `profile_local.php` vẫn còn ở thư mục gốc. Tài nguyên giao diện nằm trong `css/`, `js/`, `fonts/`, `images/` và `picture/` theo đường dẫn hiện dùng.

## Cơ sở dữ liệu

Đọc [database/README.md](database/README.md) để biết cách khởi tạo MySQL và áp dụng migration.
