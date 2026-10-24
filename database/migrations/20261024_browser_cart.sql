IF OBJECT_ID(N'dbo.CoffeeShopBrowserCart', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CoffeeShopBrowserCart
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_CoffeeShopBrowserCart PRIMARY KEY,
        CartJson nvarchar(max) NOT NULL,
        UpdatedAtUtc datetime2 NOT NULL
    );
END
GO
