# MySQL database

The application uses the `coffee_shop_db` MySQL database through XAMPP.

## Initialize a new database

1. Create the database with `utf8mb4` encoding.
2. Import [`coffee_shop_db.sql`](coffee_shop_db.sql) using phpMyAdmin or the MySQL CLI.
3. Set the local connection options in the root `config.local.php` file. Do not add local credentials to source control.

## Existing databases

Apply the SQL files in `migrations/` in filename order when they have not already been applied:

- `20261006_customer_management.sql`: customer account status and email fields.
- `20261008_crm_surveys.sql`: customer profile fields and survey invitation uniqueness.
- `20261008_product_variants.sql`: product-size variants and the optional order-detail variant reference. On older schemas, it backfills variants from legacy product size/price columns without dropping those columns.
- `20261013_staff_accounts.sql`: staff account updates.

The product-variant migration is safe to rerun. Review a database backup before applying schema migrations to production data.

The root `connect.php` provides the PDO connection used by storefront/CRM code. `database/connect.php` retains the MySQLi adapter required by the existing product and admin models.
