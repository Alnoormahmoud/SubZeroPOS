using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
    {
        public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
        {
            builder.ToTable("ExpenseCategories");

            builder.HasKey(ec => ec.ExpenseCategoryId);

            builder.Property(ec => ec.NameAr)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(true);

            // Configure the relationship with Expense entity
            builder.HasMany(ec => ec.Expenses)
                .WithOne(e => e.ExpenseCategory)
                .HasForeignKey(e => e.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
