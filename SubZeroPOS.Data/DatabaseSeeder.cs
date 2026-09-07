using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(SubZeroDbContext db)
        {

            // ============================================================
            // ROLES
            // ============================================================

            var managerRole = await db.Set<Role>()
                .FirstOrDefaultAsync(r => r.RoleCode == "Manager");

            if (managerRole == null)
            {
                managerRole = new Role
                {
                    RoleCode = "Manager",
                    RoleNameAr = "مدير"
                };

                db.Set<Role>().Add(managerRole);
            }

            var cashierRole = await db.Set<Role>()
                .FirstOrDefaultAsync(r => r.RoleCode == "Cashier");

            if (cashierRole == null)
            {
                cashierRole = new Role
                {
                    RoleCode = "Cashier",
                    RoleNameAr = "كاشير"
                };

                db.Set<Role>().Add(cashierRole);
            }

            await db.SaveChangesAsync();

            // ============================================================
            // INITIAL MANAGER USER
            // ============================================================

            var existingManager = await db.Set<User>()
                .FirstOrDefaultAsync(u => u.Username == "عماد");

            if (existingManager == null)
            {
                var ManagerRole = await db.Set<Role>()
                    .FirstAsync(r => r.RoleCode == "Manager");

                var managerUser = new User
                {
                    FullName = "عماد الطاهر",
                    Username = "عماد",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345"),
                    RoleId = ManagerRole.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                db.Set<User>().Add(managerUser);

                await db.SaveChangesAsync();
            }

            // ============================================================
            // EXPENSE CATEGORIES
            // ============================================================

            var expenseCategories = new[]
            {
    new
    {
        NameAr = "كهرباء"
    },
    new
    {
        NameAr = "مياه"
    },
    new
    {
        NameAr = "إيجار"
    },
    new
    {
        NameAr = "مشتريات"
    },
    new
    {
        NameAr = "صيانة"
    },
    new
    {
        NameAr = "نظافة"
    },
    new
    {
        NameAr = "مواصلات"
    },
    new
    {
        NameAr = "أخرى"
    }
};

            foreach (var categoryData in expenseCategories)
            {
                var existingCategory = await db.Set<ExpenseCategory>()
                    .FirstOrDefaultAsync(
                        c => c.NameAr == categoryData.NameAr);

                if (existingCategory == null)
                {
                    db.Set<ExpenseCategory>().Add(new ExpenseCategory
                    {
                        NameAr = categoryData.NameAr
                    });
                }
            }

            await db.SaveChangesAsync();


            // ============================================================
            // SAMPLE EXPENSES
            // ============================================================

            var manager = await db.Set<User>()
                .FirstAsync(u => u.Username == "admin" || u.Username == "عماد");


            // كهرباء
            await SeedExpenseAsync(
                db,
                "كهرباء",
                "فاتورة الكهرباء الشهرية",
                85000m,
                DateTime.Now.AddDays(-10),
                manager.UserId);

            // مياه
            await SeedExpenseAsync(
                db,
                "مياه",
                "فاتورة المياه",
                25000m,
                DateTime.Now.AddDays(-8),
                manager.UserId);

            // مشتريات
            await SeedExpenseAsync(
                db,
                "مشتريات",
                "شراء مواد غذائية للمطعم",
                150000m,
                DateTime.Now.AddDays(-6),
                manager.UserId);

            // نظافة
            await SeedExpenseAsync(
                db,
                "نظافة",
                "شراء مواد وأدوات تنظيف",
                35000m,
                DateTime.Now.AddDays(-5),
                manager.UserId);

            // صيانة
            await SeedExpenseAsync(
                db,
                "صيانة",
                "صيانة الثلاجة",
                60000m,
                DateTime.Now.AddDays(-3),
                manager.UserId);

            // مواصلات
            await SeedExpenseAsync(
                db,
                "مواصلات",
                "مصاريف توصيل وشراء احتياجات",
                20000m,
                DateTime.Now.AddDays(-2),
                manager.UserId);

            // ============================================================
            // RESTAURANT SETTINGS
            // ============================================================

            var settings = await db.Set<RestaurantSettings>()
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new RestaurantSettings
                {
                    // Restaurant information
                    RestaurantName = "ساب زيرو",
                    Phone = "249119881277+",
                    Address = "الخرطوم، سوبا الآراضي",

                    // Invoice footer
                    InvoiceFooterPrimary = "شكراً لزيارتكم",
                    InvoiceFooterSecondary = "نتمنى لكم يوماً سعيداً",

                    // Currency
                    CurrencyCode = "SDG",
                    CurrencySymbol = "ج.س",

                    // Date and time
                    DateFormat = "hh:mm dd-MM-yyyy",

                    // Invoice options
                    ShowCashierNameOnInvoice = true,
                    ShowLogoOnInvoice = true,
                    ShowOrderNumberOnInvoice = true,
                    ShowCustomerNameOnInvoice = true,

                    // Receipt
                    ReceiptPaperWidthMm = 80,
                    ReceiptCopies = 1,

                    // Printing
                    AutoPrintReceipt = true,
                    OpenPdfAfterPrinting = true,

                    PrinterName = string.Empty,

                    // Theme
                    Theme = "Blue Dark",

                    // Weekly backup
                    WeeklyBackupEnabled = true,
                };

                db.Set<RestaurantSettings>().Add(settings);

                await db.SaveChangesAsync();
            }

            // ============================================================
            // CATEGORIES
            // ============================================================

            var categories = new[]
            {
                new
                {
                    NameAr = "العصائر",
                    DisplayOrder = 1
                },
                new
                {
                    NameAr = "البيتزا",
                    DisplayOrder = 2
                },
                new
                {
                    NameAr = "الفطائر",
                    DisplayOrder = 3
                },
                new
                {
                    NameAr = "الحلويات",
                    DisplayOrder = 4
                },
                new
                {
                    NameAr = "السندوتشات",
                    DisplayOrder = 5
                },
                new
                {
                    NameAr = "آيس كريم",
                    DisplayOrder = 6
                },
                new
                {
                    NameAr = "المشروبات الغازية",
                    DisplayOrder = 7
                }
            };

            foreach (var categoryData in categories)
            {
                var category = await db.Set<Category>()
                    .FirstOrDefaultAsync(
                        c => c.NameAr == categoryData.NameAr);

                if (category == null)
                {
                    db.Set<Category>().Add(new Category
                    {
                        NameAr = categoryData.NameAr,
                        NameEn = null,
                        DisplayOrder = categoryData.DisplayOrder,
                        IsActive = true
                    });
                }
            }

            await db.SaveChangesAsync();

            // ============================================================
            // ORDER TYPES
            // ============================================================

            var orderTypes = new[]
            {
    new
    {
    
              TypeCode = "DINE-IN",
        NameAr = "محلي"
    },
    new
    {
         TypeCode = "TAKE-AWAY",
        NameAr = "سفري"
    },
    new
    {
        TypeCode = "DELIVERY",
        NameAr = "توصيل"
    }
            }; 

            foreach (var orderTypeData in orderTypes)
            {
                var existingOrderType = await db.Set<OrderType>()
                    .FirstOrDefaultAsync(
                        o => o.TypeCode == orderTypeData.TypeCode);

                if (existingOrderType == null)
                {
                    db.Set<OrderType>().Add(new OrderType
                    {
                        TypeCode = orderTypeData.TypeCode,
                        NameAr = orderTypeData.NameAr
                    });
                }
            }

            await db.SaveChangesAsync();


            // ============================================================
            // ITEMS
            // ============================================================

            await SeedItemAsync(
                db,
                "العصائر",
                "منقة",
                4000m);

            await SeedItemAsync(
                db,
                "العصائر",
                "موز بالحليب",
                3000m);

            await SeedItemAsync(
                db,
                "العصائر",
                "فراولة",
                4000m);

            await SeedItemAsync(
                db,
                "العصائر",
                "مشكلة",
                5000m);

            await SeedItemAsync(
                db,
                "العصائر",
                "برتقال",
                3000m);

            await SeedItemAsync(
                db,
                "العصائر",
                "سلطة فواكه",
                4000m);


            // ------------------------------------------------------------
            // PIZZA
            // ------------------------------------------------------------

            await SeedItemAsync(
                db,
                "البيتزا",
                "بيتزا بالخضار كبير",
                20000m);

            await SeedItemAsync(
                db,
                "البيتزا",
                "بيتزا بالخضار وسط",
                18000m);

            await SeedItemAsync(
                db,
                "البيتزا",
                "بيتزا بالخضار صغير",
                15000m);

            await SeedItemAsync(
                db,
                "البيتزا",
                "بيتزا بالهوت دوك كبير",
                25000m);

            await SeedItemAsync(
                db,
                "البيتزا",
                "بيتزا بالهوت دوك وسط",
                18000m);

            await SeedItemAsync(
                db,
                "البيتزا",
                "بيتزا بالهوت دوك صغير",
                15000m);


            // ------------------------------------------------------------
            // PASTRIES
            // ------------------------------------------------------------

            await SeedItemAsync(
                db,
                "الفطائر",
                "شامية",
                7000m);


            // ------------------------------------------------------------
            // DESSERTS
            // ------------------------------------------------------------

            await SeedItemAsync(
                db,
                "الحلويات",
                "كيلو باسطة",
                12000m);

            await SeedItemAsync(
                db,
                "الحلويات",
                "كنافة كيلو",
                20000m);

            await SeedItemAsync(
                db,
                "الحلويات",
                "بسبوسة",
                15000m);

            await SeedItemAsync(
                db,
                "الحلويات",
                "توينز بالزلابية",
                4000m);


            // ------------------------------------------------------------
            // SANDWICHES
            // ------------------------------------------------------------

            await SeedItemAsync(
                db,
                "السندوتشات",
                "طعميه بشبسي",
                3000m);

            await SeedItemAsync(
                db,
                "السندوتشات",
                "طعميه بخضار",
                2000m);

            await SeedItemAsync(
                db,
                "السندوتشات",
                "بيرقر كبير",
                10000m);

            await SeedItemAsync(
                db,
                "السندوتشات",
                "بيرقر وسط",
                5000m);

            await SeedItemAsync(
                db,
                "السندوتشات",
                "برست كامل",
                35000m);

            await SeedItemAsync(
                db,
                "السندوتشات",
                "برست وسط",
                18000m);


            // ------------------------------------------------------------
            // ICE CREAM
            // ------------------------------------------------------------

            await SeedItemAsync(
                db,
                "آيس كريم",
                "بوظة",
                1000m);

            await SeedItemAsync(
                db,
                "آيس كريم",
                "شعلة",
                1000m);

            await SeedItemAsync(
                db,
                "آيس كريم",
                "علبة",
                2000m);

            await SeedItemAsync(
                db,
                "آيس كريم",
                "آيس كوفي",
                4000m);


            // ------------------------------------------------------------
            // SOFT DRINKS
            // ------------------------------------------------------------

            await SeedItemAsync(
                db,
                "المشروبات الغازية",
                "كنزا",
                2000m);

            await SeedItemAsync(
                db,
                "المشروبات الغازية",
                "ريد بول",
                3000m);
        }

        // ================================================================
        // SEED EXPENSE
        // ================================================================

        private static async Task SeedExpenseAsync(
            SubZeroDbContext db,
            string categoryName,
            string description,
            decimal amount,
            DateTime expenseDate,
            int enteredByUserId)
        {
            var category = await db.Set<ExpenseCategory>()
                .FirstAsync(c => c.NameAr == categoryName);

            var existingExpense = await db.Set<Expense>()
                .FirstOrDefaultAsync(e =>
                    e.ExpenseCategoryId == category.ExpenseCategoryId &&
                    e.Description == description &&
                    e.Amount == amount);

            if (existingExpense != null)
                return;

            var expense = new Expense
            {
                ExpenseCategoryId = category.ExpenseCategoryId,
                Description = description,
                Amount = amount,
                ExpenseDate = expenseDate,
                EnteredByUserId = enteredByUserId
            };

            db.Set<Expense>().Add(expense);

            await db.SaveChangesAsync();
        }


        // ================================================================
        // SEED ITEM
        // ================================================================

        private static async Task SeedItemAsync(
            SubZeroDbContext db,
            string categoryName,
            string itemName,
            decimal price)
        {
            var existingItem = await db.Set<Item>()
                .FirstOrDefaultAsync(i => i.ItemName == itemName);

            if (existingItem != null)
            {
                // If this seeded item doesn't have an image path yet,
                // assign one based on its actual database ID.
                if (string.IsNullOrWhiteSpace(existingItem.ImagePath))
                {
                    existingItem.ImagePath =
                        $"Images/Items/{existingItem.ItemId}.jpg";

                    await db.SaveChangesAsync();
                }

                return;
            }

            var category = await db.Set<Category>()
                .FirstAsync(c => c.NameAr == categoryName);

            var item = new Item
            {
                CategoryId = category.CategoryId,
                ItemName = itemName,
                NameEn = null,
                ImagePath = null,
                Price = price,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Set<Item>().Add(item);

            // First save so SQL Server generates ItemId.
            await db.SaveChangesAsync();

            // Now we know the actual ItemId.
            item.ImagePath =
                $"Images/Items/{item.ItemId}.jpg";

            await db.SaveChangesAsync();
        }
    }
}
 