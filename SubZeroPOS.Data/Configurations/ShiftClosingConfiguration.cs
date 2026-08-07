using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class ShiftClosingConfiguration : IEntityTypeConfiguration<ShiftClosing>
    {
        public void Configure(EntityTypeBuilder<ShiftClosing> builder)
        {
            builder.ToTable("ShiftClosings");

            builder.HasKey(s => s.ShiftId);

            builder.Property(s => s.OpenedAt)
                .IsRequired();

            builder.Property(s => s.OpeningCash)
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);

            builder.Property(s => s.ExpectedCash)
                .HasColumnType("decimal(10,2)");

            builder.Property(s => s.ActualCash)
                .HasColumnType("decimal(10,2)");

            builder.Property(s => s.Notes)
                .HasMaxLength(300)
                .IsUnicode(true);

            builder.HasOne(s => s.User)
                .WithMany(u => u.ShiftClosings)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
