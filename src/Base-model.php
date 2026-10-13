<?php
// src/base-model.php
// Class cha cho các class thao tác ghi dữ liệu (Cart, Order...).
// Cần include database/connect.php trước.

abstract class BaseModel
{
    protected $db;

    public function __construct()
    {
        $this->db = new Database();
    }

    // Chạy 1 câu INSERT / UPDATE / DELETE có tham số. Trả về số dòng bị ảnh hưởng.
    // Lỗi thì ném Exception để nơi gọi rollback transaction.
    protected function chay($sql, $types = '', ...$params)
    {
        $conn = $this->db->getConnection();

        $stmt = $conn->prepare($sql);
        if (!$stmt) {
            throw new Exception($conn->error);
        }
        if ($types !== '') {
            $stmt->bind_param($types, ...$params);
        }
        if (!$stmt->execute()) {
            $loi = $stmt->error;
            $stmt->close();
            throw new Exception($loi);
        }
        $soDong = $stmt->affected_rows;
        $stmt->close();
        return $soDong;
    }

    // Id của dòng vừa INSERT gần nhất
    protected function idMoiNhat()
    {
        return (int)$this->db->getConnection()->insert_id;
    }
}