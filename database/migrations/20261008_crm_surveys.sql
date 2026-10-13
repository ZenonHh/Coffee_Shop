SET @customerBirthDateColumnSql = IF(
    EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'KhachHang' AND COLUMN_NAME = 'NgaySinh'
    ),
    'SELECT 1',
    'ALTER TABLE KhachHang ADD COLUMN NgaySinh DATE NULL AFTER Email'
);
PREPARE customerBirthDateColumnStatement FROM @customerBirthDateColumnSql;
EXECUTE customerBirthDateColumnStatement;
DEALLOCATE PREPARE customerBirthDateColumnStatement;

SET @customerInterestsColumnSql = IF(
    EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'KhachHang' AND COLUMN_NAME = 'SoThich'
    ),
    'SELECT 1',
    'ALTER TABLE KhachHang ADD COLUMN SoThich VARCHAR(500) NULL AFTER NgaySinh'
);
PREPARE customerInterestsColumnStatement FROM @customerInterestsColumnSql;
EXECUTE customerInterestsColumnStatement;
DEALLOCATE PREPARE customerInterestsColumnStatement;

SET @invitationUniqueIndexSql = IF(
    EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LoiMoiKhaoSat' AND INDEX_NAME = 'uq_loimoi_survey_customer'
    ),
    'SELECT 1',
    'ALTER TABLE LoiMoiKhaoSat ADD UNIQUE KEY uq_loimoi_survey_customer (MaKhaoSat, MaKH)'
);
PREPARE invitationUniqueIndexStatement FROM @invitationUniqueIndexSql;
EXECUTE invitationUniqueIndexStatement;
DEALLOCATE PREPARE invitationUniqueIndexStatement;
