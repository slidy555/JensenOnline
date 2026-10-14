using JENSENOnline.API.Data;
using JENSENOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace JENSENOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext db;

    public ProductsController(AppDbContext context)
    {
        db = context;
    }

    [HttpGet]
    public IActionResult GetProducts()
    {
        return Ok(db.Products.ToList());
    }

    [HttpGet("search")]
    public IActionResult Search(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Skriv ett produktnamn.");

        return Ok(db.Products
            .Where(p => p.Name.Contains(name))
            .ToList());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult AddProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) ||
            product.Price < 0 || product.Stock < 0)
        {
            return BadRequest("Ogiltig produkt.");
        }

        product.Id = 0;
        db.Products.Add(product);
        db.SaveChanges();

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, Product updated)
    {
        var product = db.Products.Find(id);

        if (product == null)
            return NotFound("Produkten finns inte.");

        if (string.IsNullOrWhiteSpace(updated.Name) ||
            updated.Price < 0 || updated.Stock < 0)
        {
            return BadRequest("Ogiltig produkt.");
        }

        product.Name = updated.Name;
        product.Description = updated.Description;
        product.Price = updated.Price;
        product.Stock = updated.Stock;

        db.SaveChanges();

        return Ok(product);
    }
}