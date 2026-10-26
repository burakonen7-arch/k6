using FlashSale.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FlashSale.Api.Controllers;
[ApiController, Route("health")]
public sealed class OperationsController(StoreDbContext db) : ControllerBase
{
 [HttpGet("live")]
 public IActionResult Live() => Ok(new { status = "healthy" });
 [HttpGet("ready")]
 public async Task<IActionResult> Ready(CancellationToken ct) => await db.Database.CanConnectAsync(ct)
  ? Ok(new { status = "ready" }) : StatusCode(503);
}
