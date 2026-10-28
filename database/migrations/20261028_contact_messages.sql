IF OBJECT_ID(N'dbo.LienHe', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LienHe
    (
        MaLienHe INT IDENTITY(1,1) NOT NULL,
        HoTen NVARCHAR(100) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        ChuDe NVARCHAR(255) NULL,
        NoiDung NVARCHAR(MAX) NOT NULL,
        NgayGui DATETIME2 NOT NULL CONSTRAINT DF_LienHe_NgayGui DEFAULT (GETDATE()),
        TrangThai NVARCHAR(50) NOT NULL CONSTRAINT DF_LienHe_TrangThai DEFAULT (N'Chưa xử lý'),
        GhiChu NVARCHAR(500) NULL,
        CONSTRAINT PK_LienHe PRIMARY KEY (MaLienHe)
    );
END;
GO
