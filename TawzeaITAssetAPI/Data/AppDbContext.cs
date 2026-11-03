using Microsoft.EntityFrameworkCore;
using TawzeaITAssetAPI.Models;

namespace TawzeaITAssetAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ITAsset>()
            .Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Lets the controller map duplicate serials (SQL 2601) to 409 Conflict
        modelBuilder.Entity<ITAsset>()
            .HasIndex(a => a.SerialNumber)
            .IsUnique();
    }


    public virtual DbSet<ITAsset> ITAssets { get; set; }

}
