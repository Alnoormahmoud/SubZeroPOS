USE SubZeroPOS;
GO

ALTER TABLE RestaurantSettings
ADD CurrencyCode VARCHAR(10) NOT NULL DEFAULT 'SDG',
    CurrencySymbol NVARCHAR(10) NOT NULL DEFAULT N'ج.س';
GO
