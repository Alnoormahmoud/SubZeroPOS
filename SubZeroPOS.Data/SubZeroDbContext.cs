using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data
{
    public class SubZeroDbContext : DbContext
    {
        public SubZeroDbContext(DbContextOptions<SubZeroDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<OrderType> OrderTypes => Set<OrderType>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ShiftClosing> ShiftClosings => Set<ShiftClosing>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Picks up every IEntityTypeConfiguration<T> class in this assembly
            // (Configurations/RoleConfiguration.cs, UserConfiguration.cs, etc.)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubZeroDbContext).Assembly);
        }
    }
}
