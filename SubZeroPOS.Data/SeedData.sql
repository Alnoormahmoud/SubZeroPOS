USE SubZeroPOS;
GO

/* ============ ROLES ============ */
INSERT INTO Roles (RoleCode, RoleNameAr) VALUES
('Manager', N'مدير'),
('Cashier', N'كاشير');
GO

/* ============ ORDER TYPES ============ */
INSERT INTO OrderTypes (TypeCode, NameAr) VALUES
('DineIn',   N'صالة'),
('Delivery', N'توصيل'),
('Pickup',   N'استلام');
GO

/* ============ EXPENSE CATEGORIES ============ */
INSERT INTO ExpenseCategories (NameAr) VALUES
(N'مواد خام'), (N'إيجار'), (N'رواتب'), (N'كهرباء وغاز'), (N'صيانة'), (N'توصيل'), (N'أخرى');
GO

/* ============ CATEGORIES ============ */
INSERT INTO Categories (NameAr, NameEn, DisplayOrder) VALUES
(N'العصائر',          'Juices',      1),
(N'البيتزا',          'Pizza',       2),
(N'الفطائر',          'Pies',        3),
(N'الحلويات',         'Sweets',      4),
(N'السندوتشات',       'Sandwiches',  5),
(N'آيس كريم',         'Ice Cream',   6),
(N'المشروبات الغازية', 'Soft Drinks', 7);
GO

/* ============ ITEMS (uses ItemName - matches your current entity) ============ */

-- Juices
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'منقة', 4.000 FROM Categories WHERE NameEn = 'Juices';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'موز بالحليب', 3.000 FROM Categories WHERE NameEn = 'Juices';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'فراولة', 4.000 FROM Categories WHERE NameEn = 'Juices';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'مشكلة', 5.000 FROM Categories WHERE NameEn = 'Juices';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'برتقال', 3.000 FROM Categories WHERE NameEn = 'Juices';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'سلطة فواكه', 4.000 FROM Categories WHERE NameEn = 'Juices';

-- Pizza
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بيتزا بالخضار كبير', 20.000 FROM Categories WHERE NameEn = 'Pizza';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بيتزا بالخضار وسط', 18.000 FROM Categories WHERE NameEn = 'Pizza';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بيتزا بالخضار صغير', 15.000 FROM Categories WHERE NameEn = 'Pizza';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بيتزا بالهوت دوك كبير', 25.000 FROM Categories WHERE NameEn = 'Pizza';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بيتزا بالهوت دوك وسط', 18.000 FROM Categories WHERE NameEn = 'Pizza';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بيتزا بالهوت دوك صغير', 15.000 FROM Categories WHERE NameEn = 'Pizza';

-- Pies
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'شامية', 7.000 FROM Categories WHERE NameEn = 'Pies';

-- Sweets
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'كيلو باسطة', 12.000 FROM Categories WHERE NameEn = 'Sweets';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'كنافة كيلو', 20.000 FROM Categories WHERE NameEn = 'Sweets';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بسبوسة', 15.000 FROM Categories WHERE NameEn = 'Sweets';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'توينز بالزلابية', 4.000 FROM Categories WHERE NameEn = 'Sweets';

-- Sandwiches
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'طعمية بشيبي', 3.000 FROM Categories WHERE NameEn = 'Sandwiches';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'طعمية بخضار', 2.000 FROM Categories WHERE NameEn = 'Sandwiches';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'برقر كبير', 10.000 FROM Categories WHERE NameEn = 'Sandwiches';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'برقر وسط', 5.000 FROM Categories WHERE NameEn = 'Sandwiches';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'برست كامل', 35.000 FROM Categories WHERE NameEn = 'Sandwiches';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'برست وسط', 18.000 FROM Categories WHERE NameEn = 'Sandwiches';

-- Ice Cream
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'بوظة', 1.000 FROM Categories WHERE NameEn = 'Ice Cream';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'شعلة', 1.000 FROM Categories WHERE NameEn = 'Ice Cream';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'علبة', 2.000 FROM Categories WHERE NameEn = 'Ice Cream';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'آيس كوفي', 4.000 FROM Categories WHERE NameEn = 'Ice Cream';

-- Soft Drinks
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'كنزا', 2.000 FROM Categories WHERE NameEn = 'Soft Drinks';
INSERT INTO Items (CategoryId, ItemName, Price)
SELECT CategoryId, N'ريد بول', 3.000 FROM Categories WHERE NameEn = 'Soft Drinks';
GO

/* ============ USERS ============
   Password hashes below are BCrypt hashes for testing:
   admin    -> Admin@123
   cashier1 -> Cashier@123
   Change these before real use.
*/
INSERT INTO Users (FullName, Username, PasswordHash, RoleId)
SELECT N'النور محمود ادم', 'admin',
       '$2b$12$/ob8vXipM9F.QuZw6tUYRebswgF6phjTfVGivWI0CWLWciVEUHp/a',
       RoleId FROM Roles WHERE RoleCode = 'Manager';

INSERT INTO Users (FullName, Username, PasswordHash, RoleId)
SELECT N'عماد الطاهر', 'cashier1',
       '$2b$12$u.g5sOckGT53pRNa7Z08j./.pUNZVeS//AuCd3JE2ExTOnLiMYoz.',
       RoleId FROM Roles WHERE RoleCode = 'Cashier';
GO

PRINT 'Seed data inserted successfully.';
