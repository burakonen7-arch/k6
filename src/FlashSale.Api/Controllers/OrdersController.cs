using FlashSale.Application;
using Microsoft.AspNetCore.Mvc;
namespace FlashSale.Api.Controllers;
[ApiController, Route("api/orders")]
public sealed class OrdersController(IOrderService service) : ControllerBase
{
 [HttpPost]
 [ProducesResponseType<OrderDto>(201)]
 [ProducesResponseType<ProblemDetails>(409)]
 public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
 {
  var order = await service.PlaceAsync(request, ct);
  return order is null ? Problem(statusCode:409, title:"Product unavailable or insufficient stock")
   : CreatedAtAction(nameof(Get), new { id = order.Id }, order);
 }
 [HttpGet("{id:guid}")]
 public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken ct)
 { var order = await service.GetAsync(id,ct); return order is null ? NotFound() : Ok(order); }
}
