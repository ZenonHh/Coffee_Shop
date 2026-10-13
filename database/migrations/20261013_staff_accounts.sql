INSERT INTO vaitro (MaVaiTro, TenVaiTro, MoTa)
SELECT 1, 'Admin', 'Quản trị hệ thống'
WHERE NOT EXISTS (
    SELECT 1 FROM vaitro WHERE LOWER(TenVaiTro) = 'admin'
);

INSERT INTO vaitro (MaVaiTro, TenVaiTro, MoTa)
SELECT 2, 'Staff', 'Nhân viên quản lý cửa hàng'
WHERE NOT EXISTS (
    SELECT 1 FROM vaitro WHERE LOWER(TenVaiTro) = 'staff'
);

