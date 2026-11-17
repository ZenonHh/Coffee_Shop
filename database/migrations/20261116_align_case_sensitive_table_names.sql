USE CoffeeShopDb;
GO

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS
    (
        SELECT 1
        FROM sys.tables AS tableInfo
        INNER JOIN sys.schemas AS schemaInfo ON schemaInfo.schema_id = tableInfo.schema_id
        WHERE schemaInfo.name = N'dbo'
            AND tableInfo.name COLLATE Latin1_General_100_BIN2 = N'chitiethoadon'
    )
        EXEC sys.sp_rename N'dbo.chitiethoadon', N'ChiTietHoaDon';

    IF EXISTS
    (
        SELECT 1
        FROM sys.tables AS tableInfo
        INNER JOIN sys.schemas AS schemaInfo ON schemaInfo.schema_id = tableInfo.schema_id
        WHERE schemaInfo.name = N'dbo'
            AND tableInfo.name COLLATE Latin1_General_100_BIN2 = N'chitiethoadon_topping'
    )
        EXEC sys.sp_rename N'dbo.chitiethoadon_topping', N'ChiTietHoaDon_Topping';

    IF EXISTS
    (
        SELECT 1
        FROM sys.tables AS tableInfo
        INNER JOIN sys.schemas AS schemaInfo ON schemaInfo.schema_id = tableInfo.schema_id
        WHERE schemaInfo.name = N'dbo'
            AND tableInfo.name COLLATE Latin1_General_100_BIN2 = N'phanhoi'
    )
        EXEC sys.sp_rename N'dbo.phanhoi', N'PhanHoi';

    COMMIT TRANSACTION;
    PRINT N'Đã đồng bộ tên bảng với các entity EF Core.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
