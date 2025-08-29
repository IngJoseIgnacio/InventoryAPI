using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly InventoryContext _context;

        public ProductsController(InventoryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            var products = await _context.Products.Include(p => p.InventoryEntries).ToListAsync();

            var response = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                InventoryEntries = p.InventoryEntries.Select(e => new InventoryEntryResponseDto
                {
                    Id = e.Id,
                    ProductId = e.ProductId,
                    Quantity = e.Quantity,
                    ExpirationDate = e.ExpirationDate,
                    EntryDate = e.EntryDate,
                    IsOutput = e.IsOutput
                }).ToList()
            }).ToList();

            return response;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.InventoryEntries)
                                                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                InventoryEntries = product.InventoryEntries.Select(e => new InventoryEntryResponseDto
                {
                    Id = e.Id,
                    ProductId = e.ProductId,
                    Quantity = e.Quantity,
                    ExpirationDate = e.ExpirationDate,
                    EntryDate = e.EntryDate,
                    IsOutput = e.IsOutput
                }).ToList()
            };

            return response;
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> PostProduct(ProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                InventoryEntries = new List<InventoryEntryResponseDto>()
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
        }

        [HttpGet("status")]
        public async Task<ActionResult<IEnumerable<ProductStatusDto>>> GetProductsStatus()
        {
            var today = DateTime.Today;
            var products = await _context.Products
                .Include(p => p.InventoryEntries)
                .ToListAsync();

            var result = new List<ProductStatusDto>();

            foreach (var product in products)
            {
                var validEntries = product.InventoryEntries
                    .Where(e => !e.IsOutput && e.ExpirationDate >= today)
                    .OrderBy(e => e.ExpirationDate)
                    .ToList();

                var totalQuantity = validEntries.Sum(e => e.Quantity);

                if (totalQuantity == 0)
                {
                    result.Add(new ProductStatusDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        TotalQuantity = 0,
                        Status = "Sin existencias",
                        NextExpiration = null
                    });
                    continue;
                }

                var nextExpiration = validEntries.Min(e => e.ExpirationDate);
                string status;

                if (nextExpiration < today)
                {
                    status = "Vencido";
                }
                else if (nextExpiration <= today.AddDays(3))
                {
                    status = "Por vencer";
                }
                else
                {
                    status = "Vigente";
                }

                result.Add(new ProductStatusDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    TotalQuantity = totalQuantity,
                    Status = status,
                    NextExpiration = nextExpiration
                });
            }

            return result;
        }
    }
}