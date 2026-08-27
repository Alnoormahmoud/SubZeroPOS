USE SubZeroPOS;
GO

/* Assumes Categories were inserted in this order (IDs 1-7):
   1=Juices, 2=Pizza, 3=Pies, 4=Sweets, 5=Sandwiches, 6=Ice Cream, 7=Soft Drinks
   Verify with: SELECT CategoryId, NameEn FROM Categories ORDER BY CategoryId;
*/

-- Juices (CategoryId = 1)
INSERT INTO Items (CategoryId, ItemName, NameEn, Price, IsActive, CreatedAt) VALUES
(1, N'منقة',        'Manga Juice',     4000, 1, GETDATE()),
(1, N'موز بالحليب',  'Banana Milk',     3000, 1, GETDATE()),
(1, N'فراولة',       'Strawberry',      4000, 1, GETDATE()),
(1, N'مشكلة',        'Mixed',           5000, 1, GETDATE()),
(1, N'برتقال',       'Orange',          3000, 1, GETDATE()),
(1, N'سلطة فواكه',   'Fruit Salad',     4000, 1, GETDATE());

-- Pizza (CategoryId = 2)
INSERT INTO Items (CategoryId, ItemName, NameEn, Price, IsActive, CreatedAt) VALUES
(2, N'بيتزا بالخضار كبير',   'Veggie Pizza Large',    20000, 1, GETDATE()),
(2, N'بيتزا بالخضار وسط',    'Veggie Pizza Medium',   18000, 1, GETDATE()),
(2, N'بيتزا بالخضار صغير',   'Veggie Pizza Small',    15000, 1, GETDATE()),
(2, N'بيتزا بالهوت دوك كبير', 'Hot Dog Pizza Large',   25000, 1, GETDATE()),
(2, N'بيتزا بالهوت دوك وسط',  'Hot Dog Pizza Medium',  18000, 1, GETDATE()),
(2, N'بيتزا بالهوت دوك صغير', 'Hot Dog Pizza Small',   15000, 1, GETDATE());

-- Pies (CategoryId = 3)
INSERT INTO Items (CategoryId, ItemName, NameEn, Price, IsActive, CreatedAt) VALUES
(3, N'شامية', 'Shamia Pie', 7000, 1, GETDATE());

-- Sweets (CategoryId = 4)
INSERT INTO Items (CategoryId, ItemName, NameEn, Price, IsActive, CreatedAt) VALUES
(4, N'كيلو باسطة',      'Basta (1kg)',        12000, 1, GETDATE()),
(4, N'كنافة كيلو',      'Kunafa (1kg)',       20000, 1, GETDATE()),
(4, N'بسبوسة',          'Basbousa',           15000, 1, GETDATE()),
(4, N'توينز بالزلابية',  'Twins with Zalabia',  4000, 1, GETDATE());

-- Sandwiches (CategoryId = 5)
INSERT INTO Items (CategoryId, ItemName, NameEn, Price, IsActive, CreatedAt) VALUES
(5, N'طعمية بشيبي',  'Falafel with Chips',  3000, 1, GETDATE()),
(5, N'طعمية بخضار',  'Falafel with Veggies', 2000, 1, GETDATE()),
(5, N'برقر كبير',    'Burger Large',        10000, 1, GETDATE()),
(5, N'برقر وسط',     'Burger Medium',        5000, 1, GETDATE()),
(5, N'برست كامل',    'Full Breast',         35000, 1, GETDATE()),
(5, N'برست وسط',     'Half Breast',         18000, 1, GETDATE());

-- Ice Cream (CategoryId = 6)
INSERT INTO Items (CategoryId, ItemName, NameEn, Price, IsActive, CreatedAt) VALUES
(6, N'بوظة',      'Ice Cream Scoop', 1000, 1, GETDATE()),
(6, N'شعلة',      'Shu''ala',        1000, 1, GETDATE()),
(6, N'علبة',      'Cup',             2000, 1, GETDATE()),
(6, N'آيس كوفي',  'Iced Coffee',     4000, 1, GETDATE());

-- Soft Drinks (CategoryId = 7)
INSERT INTO Items (CategoryId, ItemName, NameEn, Price, IsActive, CreatedAt) VALUES
(7, N'كنزا',     'Kinza',    2000, 1, GETDATE()),
(7, N'ريد بول',  'Red Bull', 3000, 1, GETDATE());
GO

PRINT 'All items inserted successfully.';
