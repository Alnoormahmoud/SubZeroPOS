using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.RoleId);

            builder.Property(r => r.RoleCode)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false); // plain ASCII code like "Manager"

            builder.HasIndex(r => r.RoleCode).IsUnique();

            builder.Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(true); // Arabic -> NVARCHAR
        }
    }
}
