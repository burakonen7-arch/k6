using FlashSale.Application;
using FlashSale.Domain;
using Microsoft.EntityFrameworkCore;
namespace FlashSale.Infrastructure;
public sealed class ProductRepository(StoreDbContext db) : IProductRepository
{
 public async Task<IReadOnlyList<ProductDto>> ListAsync(int afterId, int limit, CancellationToken ct) =>
  await db.Products.AsNoTracking().Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(limit)
   .Select(p => new ProductDto(p.Id,p.Name,p.Price,p.Stock)).ToListAsync(ct);
 public Task<ProductDto?> GetAsync(int id, CancellationToken ct) => db.Products.AsNoTracking().Where(p => p.Id == id)
  .Select(p => new ProductDto(p.Id,p.Name,p.Price,p.Stock)).SingleOrDefaultAsync(ct);
}
public sealed class OrderRepository(StoreDbContext db) : IOrderRepository
{
 public async Task<Order?> PlaceAsync(int productId, int quantity, CancellationToken ct)
 {
  await using var transaction = await db.Database.BeginTransactionAsync(ct);
  var changed = await db.Products.Where(p => p.Id == productId && p.Stock >= quantity)
   .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - quantity), ct);
  if (changed == 0) return null;
  var price = await db.Products.Where(p => p.Id == productId).Select(p => p.Price).SingleAsync(ct);
  var order = new Order { Id = Guid.NewGuid(), ProductId = productId, Quantity = quantity, Total = price * quantity, CreatedAt = DateTime.UtcNow };
  db.Orders.Add(order);
  await db.SaveChangesAsync(ct);
  await transaction.CommitAsync(ct);
  return order;
 }
 public Task<Order?> GetAsync(Guid id, CancellationToken ct) => db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id, ct);
}
