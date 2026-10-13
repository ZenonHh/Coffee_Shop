<?php
require_once __DIR__ . '/../includes/auth.php';

$wasMainAdminSession = ($_SESSION['admin_authenticated'] ?? false) === true;
unset($_SESSION['admin_user']);
unset(
    $_SESSION['admin_authenticated'],
    $_SESSION['admin_account_id'],
    $_SESSION['admin_role_id'],
    $_SESSION['admin_role_name'],
    $_SESSION['admin_username'],
    $_SESSION['admin_fullname']
);
session_regenerate_id(true);
header('Location: ' . admin_url($wasMainAdminSession ? 'index.php' : 'auth/login.php'), true, 303);
exit;
