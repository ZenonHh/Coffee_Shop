<?php
require_once __DIR__ . '/../connect.php';

function app_product_review_summaries(PDO $connection, array $productIds): array
{
    $productIds = array_values(array_unique(array_filter(
        array_map('intval', $productIds),
        static fn (int $productId): bool => $productId > 0
    )));
    if ($productIds === []) {
        return [];
    }

    $summaries = [];
    foreach ($productIds as $productId) {
        $summaries[$productId] = ['ReviewCount' => 0, 'AverageRating' => 0];
    }
    $placeholders = implode(', ', array_fill(0, count($productIds), '?'));
    $summaryQuery = $connection->prepare(
        "SELECT MaSP, COUNT(*) AS ReviewCount, AVG(DanhGia) AS AverageRating
         FROM PhanHoi
         WHERE MaSP IN ($placeholders) AND DanhGia BETWEEN 1 AND 5
         GROUP BY MaSP"
    );
    $summaryQuery->execute($productIds);
    foreach ($summaryQuery->fetchAll(PDO::FETCH_ASSOC) as $summary) {
        $summaries[(int) $summary['MaSP']] = $summary;
    }
    return $summaries;
}
