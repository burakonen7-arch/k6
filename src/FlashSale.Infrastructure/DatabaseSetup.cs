using Microsoft.EntityFrameworkCore;
namespace FlashSale.Infrastructure;
public static class DatabaseSetup
{
 public static async Task InitializeAsync(StoreDbContext db)
 {
  // Disposable lab only. Production deployments should use reviewed EF migrations.
  await db.Database.EnsureCreatedAsync();
  await db.Database.ExecuteSqlRawAsync("""
   INSERT INTO "Products" ("Id", "Name", "Price", "Stock")
   SELECT i, 'Product ' || i, 10 + (i % 10000) / 100.0, CASE WHEN i = 1 THEN 100 ELSE 100000 END
   FROM generate_series(1, 100000) AS i ON CONFLICT ("Id") DO NOTHING;
   """);
 }
}
