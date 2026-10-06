<?php

class Database
{
    private $conn = null;
    private $host = 'localhost';
    private $user = 'root';
    private $pass = '';
    private $database = 'coffee_shop_db';
    private $stmt = null;
    private $result = null;

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
        $this->result = $this->conn->query($sql);
        if (!$this->result) {
            echo 'Lỗi truy vấn: ' . $this->conn->error;
            return [];
        }
        return $this->result->fetch_all(MYSQLI_ASSOC);
    }

    public function selectOne($sql)
    {
        $this->result = $this->conn->query($sql);
        if (!$this->result) {
            return null;
        }
        return $this->result->fetch_assoc();
    }

    public function select_prepare($sql, $types = '', ...$params)
    {
        $this->stmt = $this->conn->prepare($sql);
        if (!$this->stmt) {
            die('Lỗi prepare: ' . $this->conn->error);
        }

        if ($types !== '' && !empty($params)) {
            $this->stmt->bind_param($types, ...$params);
        }

        if ($this->stmt->execute()) {
            $this->result = $this->stmt->get_result();
            return $this;
        }

        echo 'Lỗi execute: ' . $this->stmt->error;
        return false;
    }

    public function command_prepare($sql, $types = '', ...$params)
    {
        $this->stmt = $this->conn->prepare($sql);
        if (!$this->stmt) {
            die('Lỗi prepare: ' . $this->conn->error);
        }

        if ($types !== '' && !empty($params)) {
            $this->stmt->bind_param($types, ...$params);
        }

        return $this;
    }

    public function execute()
    {
        if (!$this->stmt) {
            return false;
        }

        $ok = $this->stmt->execute();
        if (!$ok) {
            echo 'Lỗi execute: ' . $this->stmt->error;
        }
        return $ok;
    }

    public function fetch()
    {
        if ($this->result && $this->result->num_rows > 0) {
            return $this->result->fetch_assoc();
        }
        return null;
    }

    public function fetchAll()
    {
        if ($this->result && $this->result->num_rows > 0) {
            return $this->result->fetch_all(MYSQLI_ASSOC);
        }
        return [];
    }

    public function numRows()
    {
        return $this->result ? $this->result->num_rows : 0;
    }
}
