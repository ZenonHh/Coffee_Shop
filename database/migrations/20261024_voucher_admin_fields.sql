IF OBJECT_ID(N'dbo.Voucher', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Voucher', N'TenChuongTrinh') IS NULL
        ALTER TABLE dbo.Voucher ADD TenChuongTrinh nvarchar(150) NULL;

    IF COL_LENGTH(N'dbo.Voucher', N'NgayBatDau') IS NULL
        ALTER TABLE dbo.Voucher ADD NgayBatDau datetime2 NULL;

    IF COL_LENGTH(N'dbo.Voucher', N'NgayKetThuc') IS NULL
        ALTER TABLE dbo.Voucher ADD NgayKetThuc datetime2 NULL;

    IF COL_LENGTH(N'dbo.Voucher', N'DonToiThieu') IS NULL
        ALTER TABLE dbo.Voucher ADD DonToiThieu decimal(12,2) NOT NULL
            CONSTRAINT DF_Voucher_DonToiThieu DEFAULT (0) WITH VALUES;
END
