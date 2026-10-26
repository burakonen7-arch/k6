namespace FlashSale.Domain;
public sealed class Product
{
 public int Id { get; set; }
 public string Name { get; set; } = "";
 public decimal Price { get; set; }
 public int Stock { get; set; }
}
public sealed class Order
{
 public Guid Id { get; set; }
 public int ProductId { get; set; }
 public int Quantity { get; set; }
 public decimal Total { get; set; }
 public DateTime CreatedAt { get; set; }
}
