using System.ComponentModel.DataAnnotations;
using FlashSale.Domain;
namespace FlashSale.Application;
public sealed record ProductDto(int Id, string Name, decimal Price, int Stock);
public sealed record OrderDto(Guid Id, int ProductId, int Quantity, decimal Total, DateTime CreatedAt);
public sealed record CreateOrderRequest([Range(1, int.MaxValue)] int ProductId, [Range(1, 100)] int Quantity);
public interface IProductRepository
{
 Task<IReadOnlyList<ProductDto>> ListAsync(int afterId, int limit, CancellationToken ct);
 Task<ProductDto?> GetAsync(int id, CancellationToken ct);
}
public interface IOrderRepository
{
 Task<Order?> PlaceAsync(int productId, int quantity, CancellationToken ct);
 Task<Order?> GetAsync(Guid id, CancellationToken ct);
}
public interface IProductService
{
 Task<IReadOnlyList<ProductDto>> ListAsync(int afterId, int limit, CancellationToken ct);
 Task<ProductDto?> GetAsync(int id, CancellationToken ct);
}
public sealed class ProductService(IProductRepository repository) : IProductService
{
 public Task<IReadOnlyList<ProductDto>> ListAsync(int afterId, int limit, CancellationToken ct) => repository.ListAsync(afterId, limit, ct);
 public Task<ProductDto?> GetAsync(int id, CancellationToken ct) => repository.GetAsync(id, ct);
}
public interface IOrderService
{
 Task<OrderDto?> PlaceAsync(CreateOrderRequest request, CancellationToken ct);
 Task<OrderDto?> GetAsync(Guid id, CancellationToken ct);
}
public sealed class OrderService(IOrderRepository repository) : IOrderService
{
 static OrderDto? Map(Order? o) => o is null ? null : new(o.Id, o.ProductId, o.Quantity, o.Total, o.CreatedAt);
 public async Task<OrderDto?> PlaceAsync(CreateOrderRequest request, CancellationToken ct) => Map(await repository.PlaceAsync(request.ProductId, request.Quantity, ct));
 public async Task<OrderDto?> GetAsync(Guid id, CancellationToken ct) => Map(await repository.GetAsync(id, ct));
}
