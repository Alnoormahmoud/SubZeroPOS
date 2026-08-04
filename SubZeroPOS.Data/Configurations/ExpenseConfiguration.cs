using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses");

            builder.HasKey(e => e.ExpenseId);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(300)
                .IsUnicode(true);

            builder.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.ExpenseDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(e => e.ExpenseCategory)
                .WithMany(ec => ec.Expenses)
                .HasForeignKey(e => e.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EnteredByUser)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.EnteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // matches: CREATE INDEX IX_Expenses_ExpenseDate ON Expenses(ExpenseDate);
            builder.HasIndex(e => e.ExpenseDate)
                .HasDatabaseName("IX_Expenses_ExpenseDate");
        }
    }
}
