# SQL Server setup

1. Confirm that the `Coffee_Shop_DB` database and the tables from `Coffee_Shop_DB.sql` exist.
2. Open `migrations/20261006_storefront.sql` in SQL Server Management Studio and execute it against `Coffee_Shop_DB`. It adds product image and delivery-recipient columns, then creates the storefront categories if they do not already exist.
3. Keep database credentials in the ignored root `config.local.php` file. A safe template is available in `config.example.php`.
4. Set a local admin password hash in `config.local.php`. Generate one with:

   ```powershell
   C:\Xampp\php\php.exe -r "echo password_hash('Choose-A-Strong-Local-Password', PASSWORD_DEFAULT), PHP_EOL;"
   ```

5. Visit `admin/index.php`, sign in with the configured admin username and password, and add products. Product images can also be selected from the existing `images/` folder. The SQL source script contains no product rows, so the menu stays empty until products exist in the database.

The customer menu only lists products with an active `TrangThai`; prices and order totals are read from SQL Server. Product uploads are stored in `images/`. Checkout requires the migration columns and saves the order and its detail rows in one transaction.
