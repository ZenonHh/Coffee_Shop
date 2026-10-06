<?php
declare(strict_types=1);

$localConfigPath = __DIR__ . '/config.local.php';
$localConfig = is_file($localConfigPath) ? require $localConfigPath : [];

if (!is_array($localConfig)) {
    throw new RuntimeException('config.local.php must return an array.');
}

$host = $localConfig['db_host'] ?? (getenv('COFFEE_DB_HOST') ?: 'localhost');
$port = $localConfig['db_port'] ?? (getenv('COFFEE_DB_PORT') ?: 3306);
$database = $localConfig['db_name'] ?? (getenv('COFFEE_DB_NAME') ?: 'coffee_shop_db');
$username = $localConfig['db_username'] ?? (getenv('COFFEE_DB_USERNAME') ?: 'root');
$password = $localConfig['db_password'] ?? (getenv('COFFEE_DB_PASSWORD') ?: '');

if (
    !is_string($host) || $host === ''
    || filter_var($port, FILTER_VALIDATE_INT) === false
    || !is_string($database) || $database === ''
    || !is_string($username) || $username === ''
    || !is_string($password)
) {
    throw new RuntimeException('MySQL connection settings are invalid.');
}

$connectionString = sprintf(
    'mysql:host=%s;port=%d;dbname=%s;charset=utf8mb4',
    $host,
    (int) $port,
    $database
);

$conn = new PDO($connectionString, $username, $password, [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
]);
