USE SubZeroPOS;
GO

CREATE TABLE RestaurantSettings (
    SettingsId INT IDENTITY(1,1) PRIMARY KEY,
    RestaurantName NVARCHAR(150) NOT NULL,
    Phone VARCHAR(20) NULL,
    Address NVARCHAR(300) NULL,
    InvoiceFooterPrimary NVARCHAR(200) NULL,
    InvoiceFooterSecondary NVARCHAR(300) NULL
);
GO

-- Seed one default row (the app will also auto-create this on first run if missing,
-- but you can pre-fill your real shop details here now)
INSERT INTO RestaurantSettings (RestaurantName, Phone, Address, InvoiceFooterPrimary, InvoiceFooterSecondary)
VALUES (N'سوب زيرو', '0119881277', N'', N'شكراً لزيارتكم', N'الوجبات لا ترد ولا تستبدل بعد الخروج من المطعم');
GO
