using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SubZeroPOS.Data
{
    /// <summary>
    /// Used only by the EF Core CLI (dotnet ef migrations add / update-database)
    /// so it can construct a DbContext without needing the full WPF app to run.
    /// This connection string is for design-time only - the real app gets its
    /// connection string from DI in SubZeroPOS.WPF/App.xaml.cs.
    /// </summary>
    public class SubZeroDbContextFactory : IDesignTimeDbContextFactory<SubZeroDbContext>
    {
        public SubZeroDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SubZeroDbContext>();

            // TODO: replace with your actual local SQL Server instance name
            //optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=SubZeroPOS;Trusted_Connection=True;TrustServerCertificate=True;");
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=SubZeroPOS;Trusted_Connection=True;TrustServerCertificate=True;");

            return new SubZeroDbContext(optionsBuilder.Options);
        }
    }
}
