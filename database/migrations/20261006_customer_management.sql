SET @customerEmailColumnSql = IF(
    EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'KhachHang'
          AND COLUMN_NAME = 'Email'
    ),
    'SELECT 1',
    'ALTER TABLE KhachHang ADD COLUMN Email VARCHAR(100) NULL AFTER Sdt'
);
PREPARE customerEmailColumnStatement FROM @customerEmailColumnSql;
EXECUTE customerEmailColumnStatement;
DEALLOCATE PREPARE customerEmailColumnStatement;

SET @customerStatusColumnSql = IF(
    EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'KhachHang'
          AND COLUMN_NAME = 'TrangThai'
    ),
    'SELECT 1',
    'ALTER TABLE KhachHang ADD COLUMN TrangThai TINYINT(1) NOT NULL DEFAULT 1 AFTER HangTV'
);
PREPARE customerStatusColumnStatement FROM @customerStatusColumnSql;
EXECUTE customerStatusColumnStatement;
DEALLOCATE PREPARE customerStatusColumnStatement;
