<?php
declare(strict_types=1);

function app_start_session(): void
{
    if (session_status() === PHP_SESSION_ACTIVE) {
        return;
    }

    session_set_cookie_params([
        'httponly' => true,
        'secure' => !empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off',
        'samesite' => 'Lax',
    ]);
    session_start();
}

function app_cart_count(): int
{
    app_start_session();
    $cart = $_SESSION['cart'] ?? [];
    if (!is_array($cart)) {
        return 0;
    }

    return array_sum(array_map(
        static fn (mixed $quantity): int => is_numeric($quantity) && (int) $quantity > 0 ? (int) $quantity : 0,
        $cart
    ));
}
