<?php

class Product
{
    private $db;

    public function __construct()
    {
        $this->db = new Database();
    }

    public function getAll()
    {
        return $this->db->select('SELECT * FROM products ORDER BY id DESC');
    }

    public function getById($id)
    {
        $id = (int) $id;
        return $this->db->selectOne("SELECT * FROM products WHERE id = $id");
    }

    public function getByCategory($categoryId)
    {
        $categoryId = (int) $categoryId;
        return $this->db->select("SELECT * FROM products WHERE category_id = $categoryId ORDER BY id DESC");
    }
}
