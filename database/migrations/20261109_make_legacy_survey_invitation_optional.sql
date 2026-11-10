IF OBJECT_ID(N'dbo.CauTraLoi', N'U') IS NOT NULL
    AND COL_LENGTH(N'dbo.CauTraLoi', N'MaLoiMoi') IS NOT NULL
    AND EXISTS
    (
        SELECT 1
        FROM sys.columns
        WHERE [object_id] = OBJECT_ID(N'dbo.CauTraLoi')
            AND [name] = N'MaLoiMoi'
            AND [is_nullable] = 0
    )
BEGIN
    ALTER TABLE dbo.CauTraLoi ALTER COLUMN MaLoiMoi int NULL;
END;
GO
