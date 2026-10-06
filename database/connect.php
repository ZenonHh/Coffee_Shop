<?php

class Database
{
    private $conn = null;
    private $host = 'localhost';
    private $user = 'root';
    private $pass = '';
    private $database = 'web_coffee';

    public function __construct()
    {
        $this->connect();
    }

    private function connect()
    {
        if ($this->conn === null) {
            $this->conn = new mysqli($this->host, $this->user, $this->pass, $this->database);

            if ($this->conn->connect_error) {
                die('Kết nối database thất bại: ' . $this->conn->connect_error);
            }

            $this->conn->set_charset('utf8mb4');
        }
    }

    public function getConnection()
    {
        return $this->conn;
    }

    public function select($sql)
    {
        $result = $this->conn->query($sql);
        if (!$result) {
            echo 'Lỗi truy vấn: ' . $this->conn->error;
            return [];
        }
        return $result->fetch_all(MYSQLI_ASSOC);
    }

    public function selectOne($sql)
    {
        $result = $this->conn->query($sql);
        if (!$result) {
            return null;
        }
        return $result->fetch_assoc();
    }
}
