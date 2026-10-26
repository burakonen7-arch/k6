using System.ComponentModel.DataAnnotations;
using FlashSale.Application;
using Microsoft.AspNetCore.Mvc;
namespace FlashSale.Api.Controllers;
[ApiController, Route("api/products")]
public sealed class ProductsController(IProductService service) : ControllerBase
{
 [HttpGet]
 public async Task<ActionResult<IReadOnlyList<ProductDto>>> List([FromQuery, Range(0,int.MaxValue)] int afterId = 0, [FromQuery, Range(1,100)] int limit = 20, CancellationToken ct = default)
  => Ok(await service.ListAsync(afterId, limit, ct));
 [HttpGet("{id:int}")]
 public async Task<ActionResult<ProductDto>> Get(int id, CancellationToken ct)
 { var product = await service.GetAsync(id,ct); return product is null ? NotFound() : Ok(product); }
}
