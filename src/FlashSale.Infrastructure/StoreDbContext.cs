using FlashSale.Domain;
using Microsoft.EntityFrameworkCore;
namespace FlashSale.Infrastructure;
public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
 public DbSet<Product> Products => Set<Product>();
 public DbSet<Order> Orders => Set<Order>();
 protected override void OnModelCreating(ModelBuilder b)
 {
  b.Entity<Product>().Property(x => x.Name).HasMaxLength(200);
  b.Entity<Product>().Property(x => x.Price).HasPrecision(18,2);
  b.Entity<Product>().ToTable(t => { t.HasCheckConstraint("CK_Product_Stock", "\"Stock\" >= 0"); t.HasCheckConstraint("CK_Product_Price", "\"Price\" >= 0"); });
  b.Entity<Order>().Property(x => x.Total).HasPrecision(18,2);
  b.Entity<Order>().HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Order>().ToTable(t => t.HasCheckConstraint("CK_Order_Quantity", "\"Quantity\" > 0"));
 }
}
