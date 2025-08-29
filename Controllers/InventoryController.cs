using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryContext _context;

        public InventoryController(InventoryContext context)
        {
            _context = context;
        }

        [HttpPost("input")]
        public async Task<ActionResult<InventoryEntryResponseDto>> AddInput(InventoryOperationDto operation)
        {
            if (operation.Quantity <= 0)
                return BadRequest("La cantidad debe ser mayor a cero");

            if (operation.ExpirationDate <= DateTime.Today)
                return BadRequest("La fecha de caducidad debe ser futura");

            var product = await _context.Products.FindAsync(operation.ProductId);
            if (product == null)
                return NotFound("Producto no encontrado");

            var entry = new InventoryEntry
            {
                ProductId = operation.ProductId,
                Quantity = operation.Quantity,
                ExpirationDate = operation.ExpirationDate,
                IsOutput = false
            };

            _context.InventoryEntries.Add(entry);
            await _context.SaveChangesAsync();

            var responseDto = new InventoryEntryResponseDto
            {
                Id = entry.Id,
                ProductId = entry.ProductId,
                Quantity = entry.Quantity,
                ExpirationDate = entry.ExpirationDate,
                EntryDate = entry.EntryDate,
                IsOutput = entry.IsOutput
            };

            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, responseDto);
        }

        [HttpPost("output")]
        public async Task<ActionResult> AddOutput(InventoryOperationDto operation)
        {
            if (operation.Quantity <= 0)
                return BadRequest("La cantidad debe ser mayor a cero");

            var product = await _context.Products
                .Include(p => p.InventoryEntries)
                .FirstOrDefaultAsync(p => p.Id == operation.ProductId);

            if (product == null)
                return NotFound("Producto no encontrado");

            var today = DateTime.Today;
            var availableEntries = product.InventoryEntries
                .Where(e => !e.IsOutput && e.ExpirationDate >= today)
                .OrderBy(e => e.ExpirationDate)
                .ToList();

            var totalAvailable = availableEntries.Sum(e => e.Quantity);

            if (totalAvailable < operation.Quantity)
                return BadRequest("No hay suficiente stock disponible");

            var remainingQuantity = operation.Quantity;
            foreach (var entry in availableEntries)
            {
                if (remainingQuantity <= 0) break;

                var quantityToDeduct = Math.Min(remainingQuantity, entry.Quantity);

                var outputEntry = new InventoryEntry
                {
                    ProductId = operation.ProductId,
                    Quantity = quantityToDeduct,
                    ExpirationDate = entry.ExpirationDate,
                    IsOutput = true,
                    EntryDate = DateTime.Now
                };

                _context.InventoryEntries.Add(outputEntry);
                remainingQuantity -= quantityToDeduct;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("entry/{id}")]
        public async Task<ActionResult<InventoryEntryResponseDto>> GetEntry(int id)
        {
            var entry = await _context.InventoryEntries.FindAsync(id);

            if (entry == null)
            {
                return NotFound();
            }

            var responseDto = new InventoryEntryResponseDto
            {
                Id = entry.Id,
                ProductId = entry.ProductId,
                Quantity = entry.Quantity,
                ExpirationDate = entry.ExpirationDate,
                EntryDate = entry.EntryDate,
                IsOutput = entry.IsOutput
            };

            return responseDto;
        }
    }
}