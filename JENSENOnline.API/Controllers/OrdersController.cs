using JENSENOnline.API.Data;
using JENSENOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace JENSENOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext db;

    public OrdersController(AppDbContext context)
    {
        db = context;
    }

    [HttpGet]
    public IActionResult GetOrders()
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return Ok(db.Orders
            .Where(o => o.UserId == userId)
            .ToList());
    }

    [HttpPost]
    public IActionResult CreateOrder(int productId, int quantity)
    {
        if (quantity <= 0)
            return BadRequest("Antalet måste vara större än noll.");

        var product = db.Products.Find(productId);

        if (product == null)
            return NotFound("Produkten finns inte.");

        if (product.Stock < quantity)
            return BadRequest("Produkten finns inte i tillräckligt antal.");

        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        product.Stock -= quantity;

        var order = new Order
        {
            UserId = userId,
            ProductId = productId,
            Quantity = quantity
        };

        db.Orders.Add(order);
        db.SaveChanges();

        return Ok(order);
    }
}