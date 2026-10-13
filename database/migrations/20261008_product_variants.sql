CREATE TABLE IF NOT EXISTS bienthesanpham (
    MaBienThe INT NOT NULL AUTO_INCREMENT,
    MaSP INT NOT NULL,
    KichCo VARCHAR(10) NOT NULL DEFAULT 'M',
    GiaBan DECIMAL(12, 2) NOT NULL,
    TrangThai TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (MaBienThe),
    CONSTRAINT fk_bienthe_sanpham
        FOREIGN KEY (MaSP) REFERENCES sanpham (MaSP) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET @legacyProductColumnsExist = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'sanpham'
      AND COLUMN_NAME IN ('KichCo', 'GiaBan')
);
SET @copyLegacyVariantsSql = IF(
    @legacyProductColumnsExist = 2,
    'INSERT INTO bienthesanpham (MaSP, KichCo, GiaBan, TrangThai)
     SELECT sp.MaSP, COALESCE(NULLIF(sp.KichCo, ''''), ''M''), sp.GiaBan, COALESCE(sp.TrangThai, 1)
     FROM sanpham AS sp
     WHERE NOT EXISTS (
         SELECT 1 FROM bienthesanpham AS bt WHERE bt.MaSP = sp.MaSP
     )',
    'SELECT 1'
);
PREPARE copyLegacyVariantsStatement FROM @copyLegacyVariantsSql;
EXECUTE copyLegacyVariantsStatement;
DEALLOCATE PREPARE copyLegacyVariantsStatement;

SET @orderVariantColumnSql = IF(
    EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'chitiethoadon' AND COLUMN_NAME = 'MaBienThe'
    ),
    'SELECT 1',
    'ALTER TABLE chitiethoadon ADD COLUMN MaBienThe INT NULL AFTER MaSP'
);
PREPARE orderVariantColumnStatement FROM @orderVariantColumnSql;
EXECUTE orderVariantColumnStatement;
DEALLOCATE PREPARE orderVariantColumnStatement;

SET @orderVariantIndexSql = IF(
    EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'chitiethoadon' AND INDEX_NAME = 'fk_chitiethoadon_bienthe'
    ),
    'SELECT 1',
    'ALTER TABLE chitiethoadon ADD INDEX fk_chitiethoadon_bienthe (MaBienThe)'
);
PREPARE orderVariantIndexStatement FROM @orderVariantIndexSql;
EXECUTE orderVariantIndexStatement;
DEALLOCATE PREPARE orderVariantIndexStatement;

SET @orderVariantForeignKeySql = IF(
    EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'chitiethoadon'
          AND COLUMN_NAME = 'MaBienThe'
          AND REFERENCED_TABLE_NAME = 'bienthesanpham'
    ),
    'SELECT 1',
    'ALTER TABLE chitiethoadon
     ADD CONSTRAINT fk_chitiethoadon_bienthe
     FOREIGN KEY (MaBienThe) REFERENCES bienthesanpham (MaBienThe)'
);
PREPARE orderVariantForeignKeyStatement FROM @orderVariantForeignKeySql;
EXECUTE orderVariantForeignKeyStatement;
DEALLOCATE PREPARE orderVariantForeignKeyStatement;
