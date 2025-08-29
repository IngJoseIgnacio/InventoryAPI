namespace InventoryAPI.Models.DTOs
{
    public class InventoryEntryResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime EntryDate { get; set; }
        public bool IsOutput { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<InventoryEntryResponseDto> InventoryEntries { get; set; } = new List<InventoryEntryResponseDto>();
    }
}